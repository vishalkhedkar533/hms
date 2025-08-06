using HMS.Data;
using HMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentMovementHistoryController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;

        public AgentMovementHistoryController(HMSContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

            return CreatedAtAction(nameof(GetById), new { id = movement.MovementId }, movement);
        }
        public async Task<IActionResult> GetById(long id)
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
    }
}
