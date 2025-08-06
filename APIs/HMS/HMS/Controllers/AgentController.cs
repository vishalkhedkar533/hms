using HMS.Data;
using HMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {

        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        public AgentController(HMSContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("Request")]
        public async Task<IActionResult> RequestTermination([FromBody] AgentTerminationRequest request)
        {
            request.Status = "Pending";
            request.RequestedDate = DateTime.UtcNow;

            _context.AgentTerminationRequest.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Termination request submitted." });
        }
        [HttpPost("Approve/{requestId}")]
        public async Task<IActionResult> ApproveTermination(int requestId)
        {
            var username = HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("User identity is not available.");

            var request = await _context.AgentTerminationRequest
                .Include(r => r.Agent)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null || request.Status != "Pending")
                return BadRequest("Invalid or already processed request.");

            var agent = request.Agent!;
            var oldStatus = agent.AgentStatusCode;

            // Perform soft delete
            agent.AgentStatusCode = "Terminated";
            agent.IsActive = false;
            agent.ModifiedBy = username;
            agent.ModifiedDate = DateTime.UtcNow;

            // Audit field-level changes
            var auditEntries = GetAgentAuditTrails(agent, username);
            _context.AgentAuditTrail.AddRange(auditEntries);

            // Update request
            request.Status = "Approved";
            request.ApprovedBy = username;
            request.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Termination approved and agent soft-deleted." });
        }

        [HttpPost("Reject/{requestId}")]
        public async Task<IActionResult> RejectTermination(int requestId, [FromBody] string? reason)
        {
            var username = HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("User identity is not available.");

            var request = await _context.AgentTerminationRequest.FindAsync(requestId);
            if (request == null || request.Status != "Pending")
                return BadRequest("Invalid or already processed request.");

            request.Status = "Rejected";
            request.RejectedBy = username;
            request.RejectedDate = DateTime.UtcNow;

            // Log rejection (optional audit)
            _context.AgentAuditTrail.Add(new AgentAuditTrail
            {
                AgentId = request.AgentId,
                FieldName = "TerminationRequestStatus",
                OldValue = "Pending",
                NewValue = "Rejected",
                ChangedBy = username,
                ChangedDate = DateTime.UtcNow,
                CreatedBy = username,
                CreatedDate = DateTime.UtcNow,
                //Remarks = reason
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Termination rejected." });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("Approve/{movementId}")]
        public async Task<IActionResult> ApproveMovement(long movementId, [FromQuery] string approverUserId)
        {
            var newMovement = await _context.agentMovementHistory
                .FirstOrDefaultAsync(m => m.MovementId == movementId);

            if (newMovement == null)
                return NotFound("Movement record not found.");

            if (newMovement.IsActive)
                return BadRequest("Movement is already active.");

            if (!string.IsNullOrEmpty(newMovement.RejectedBy))
                return BadRequest("Movement has already been rejected.");

            // Mark old active record inactive
            var oldMovement = await _context.agentMovementHistory
                .Where(m => m.AgentId == newMovement.AgentId &&
                            m.MovementId != newMovement.MovementId &&
                            m.IsActive)
                .OrderByDescending(m => m.EffectiveFromDate)
                .FirstOrDefaultAsync();

            if (oldMovement != null)
            {
                oldMovement.IsActive = false;
                oldMovement.EffectiveToDate ??= DateTime.UtcNow.Date;
                _context.agentMovementHistory.Update(oldMovement);
            }

            // Activate the new movement
            newMovement.IsActive = true;
            newMovement.EffectiveToDate = null;
            newMovement.ApprovedBy = approverUserId;
            newMovement.ApprovedDate = DateTime.UtcNow;

            _context.agentMovementHistory.Update(newMovement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Agent movement approved." });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("Reject/{movementId}")]
        public async Task<IActionResult> RejectMovement(long movementId, [FromQuery] string rejectorUserId)
        {
            var movement = await _context.agentMovementHistory
                .FirstOrDefaultAsync(m => m.MovementId == movementId);

            if (movement == null)
                return NotFound("Movement record not found.");

            if (movement.IsActive)
                return BadRequest("Cannot reject an already active movement.");

            if (!string.IsNullOrEmpty(movement.ApprovedBy))
                return BadRequest("Movement has already been approved.");

            if (!string.IsNullOrEmpty(movement.RejectedBy))
                return BadRequest("Movement has already been rejected.");

            movement.RejectedBy = rejectorUserId;
            movement.RejectedDate = DateTime.UtcNow;
            movement.IsActive = false;

            _context.agentMovementHistory.Update(movement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Agent movement rejected." });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateAgentMovement")]
        public async Task<IActionResult> CreateAgentMovement([FromBody] AgentMovementHistory movement)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Optional: Validate Agent existence
            var agentExists = await _context.agent.AnyAsync(a => a.AgentId == movement.AgentId);
            if (!agentExists)
                return NotFound($"Agent with ID {movement.AgentId} not found.");

            var newSupervisorExists = await _context.agent.AnyAsync(a => a.AgentId == movement.NewSupervisorCode);
            if (!newSupervisorExists)
                return NotFound($"New supervisor with ID {movement.NewSupervisorCode} not found.");

            if (movement.OldSupervisorCode.HasValue)
            {
                var oldSupervisorExists = await _context.agent.AnyAsync(a => a.AgentId == movement.OldSupervisorCode.Value);
                if (!oldSupervisorExists)
                    return NotFound($"Old supervisor with ID {movement.OldSupervisorCode.Value} not found.");
            }

            movement.CreatedDate = DateTime.UtcNow;
            if (movement.EffectiveFromDate < DateTime.UtcNow.Date)
            {
                movement.EffectiveFromDate = DateTime.UtcNow;
            }
            await _context.agentMovementHistory.AddAsync(movement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAgentMovementById), new { id = movement.MovementId }, movement);
        }
        public async Task<IActionResult> GetAgentMovementById(long id)
        {
            var movement = await _context.agentMovementHistory
                                         .Include(m => m.Agent)
                                         .Include(m => m.OldSupervisor)
                                         .Include(m => m.NewSupervisor)
                                         .FirstOrDefaultAsync(m => m.MovementId == id);

            if (movement == null)
                return NotFound();

            return Ok(movement);
        }
        private List<AgentAuditTrail> GetAgentAuditTrails(Agent agent, string username)
        {
            var entries = new List<AgentAuditTrail>();

            // You can expand this with any fields you want to audit
            if (agent.AgentStatusCode != "Terminated")
            {
                entries.Add(new AgentAuditTrail
                {
                    AgentId = agent.AgentId,
                    FieldName = "AgentStatusCode",
                    OldValue = agent.AgentStatusCode,
                    NewValue = "Terminated",
                    ChangedBy = username,
                    ChangedDate = DateTime.UtcNow,
                    CreatedBy = username,
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (agent.IsActive)
            {
                entries.Add(new AgentAuditTrail
                {
                    AgentId = agent.AgentId,
                    FieldName = "IsActive",
                    OldValue = "true",
                    NewValue = "false",
                    ChangedBy = username,
                    ChangedDate = DateTime.UtcNow,
                    CreatedBy = username,
                    CreatedDate = DateTime.UtcNow
                });
            }

            return entries;
        }

    }
}