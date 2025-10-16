using AutoMapper;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly DatabaseService _db;
        public AgentController(HMSContext context, IConfiguration config, IMapper mapper, DatabaseService db)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _db = db;
        }

        [HttpPost("Termination/Request")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RequestTermination([FromBody] AgentTerminationRequest request)
        {
            request.Status = "Pending";
            request.RequestedDate = DateTime.UtcNow;

            _context.AgentTerminationRequest.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Termination request submitted." });
        }
        [HttpPost("Termination/Approve/{requestId}")]
        [Authorize(Roles = "Admin")]
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
        [HttpPost("Termination/Reject/{requestId}")]
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
        [HttpPost("Movement/Approve/{movementId}")]
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
        [HttpPost("Movement/Reject/{movementId}")]
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
        [HttpPost("Movement/Request")]
        public async Task<IActionResult> RequestAgentMovement([FromBody] AgentMovementHistory movement)
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
        [HttpGet("{id}")]
        [Authorize]
        private async Task<IActionResult> GetAgentMovementById(long id)
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

        [HttpPost("Search")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> Search([FromBody] SearchAgent agentDto)
        {
            /*
            curl--location 'http://localhost:5234/api/agent/search' \
            --header 'Content-Type: application/json' \
            --header 'Authorization: Bearer ' \
            --data '{
              "agentName": "shyam"
            }'
            */

            if (agentDto.PageNo == 0)
            {
                agentDto.PageNo = 1;
            }
            if (agentDto.PageSize == 0)
            {
                agentDto.PageSize = 10;
            }
            HmsResponse hMSResponse = new HmsResponse();
            // List<AgentDto> agents = new List<AgentDto>();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var agentDtos = await _db.ExecuteQueryAsync<AgentDto>(
                "Agent",
                "Search1",
                new
                {
                    p_searchcondition = agentDto.SearchCondition,
                    p_zone = agentDto.Zone,
                    p_page_number = agentDto.PageNo,
                    p_page_size = agentDto.PageSize,
                    p_sort_column = agentDto.SortColumn,
                    p_sort_direction = agentDto.SortDirection
                });
            /*
             *     p_agent_name VARCHAR DEFAULT NULL,
    p_email VARCHAR DEFAULT NULL,
    p_mobileno VARCHAR DEFAULT NULL,
    p_pan_number VARCHAR DEFAULT NULL,
    p_aadhaar_number VARCHAR DEFAULT NULL,
    p_irda_license_number VARCHAR DEFAULT NULL,
    p_gst_number VARCHAR DEFAULT NULL,
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_sort_column VARCHAR DEFAULT 'agent_id',
    p_sort_direction VARCHAR DEFAULT 'ASC'
             */
            if (agentDtos != null)
            {
                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                hMSResponse.responseBody.agents = agentDtos.ToList();
            }
            else
            {
                hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
            }
            return agentDtos == null ? NotFound(hMSResponse) : Ok(hMSResponse);
        }
        [HttpPost("Create")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> CreateAgent([FromBody] AgentDto agentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var agent = _mapper.Map<Agent>(agentDto);
            agent.CreatedDate = DateTime.UtcNow;  // set server timestamp
            agent.IsActive = true;                // default active

            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<AgentDto>(agent);
            return CreatedAtAction(nameof(GetAgentById), new { id = agent.AgentId }, result);
        }
        [HttpPost("AgentById")]
        //[HttpPost("{id:int}")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetAgentById(SearchAgent searchAgent)
        {
            HmsResponse hMSResponse = new HmsResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            List<AgentDto> agents = new List<AgentDto>();
            AgentDto agentDTO = new AgentDto();
            IQueryable <Agent> agent = _context.Agents;

            if (searchAgent.AgentId != null)
            {
                //filter agent if agent is specified
                //else fetch all
                agent = agent.Where(x => x.AgentId == searchAgent.AgentId);
                searchAgent.FetchHierarchy = false;
            }
            var agentEntity = await agent.ToListAsync();
            if (agentEntity != null)
            {
                agentDTO = _mapper.Map<AgentDto>(agentEntity);
                if (searchAgent.FetchHierarchy)
                {
                    //where the agent is the supervisor
                    var reportees = await (
                        from h in _context.AgentHierarchies.AsNoTracking()
                        join a in _context.Agents.AsNoTracking() on h.AgentId equals a.AgentId
                        where h.SupervisorCode == searchAgent.AgentId
                        select a
                    ).ToListAsync();

                    //where the agent is the reportee
                    var supervisors = await (
                        from h in _context.AgentHierarchies.AsNoTracking()
                        join a in _context.Agents.AsNoTracking() on h.SupervisorCode equals a.AgentId
                        where h.AgentId == searchAgent.AgentId
                        select a
                    ).ToListAsync();

                    List<AgentDto> supervisorsDTO = _mapper.Map<List<AgentDto>>(supervisors);
                    List<AgentDto> reporteesDTO = _mapper.Map<List<AgentDto>>(reportees);
              
                    agentDTO.Supervisors = supervisorsDTO;
                    agentDTO.Reportees = reporteesDTO;
                }
                agents.Add(agentDTO);
                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                hMSResponse.responseBody.agents = agents;
            }
            else
            {
                hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
            }
            return agentEntity == null ? NotFound(hMSResponse) : Ok(hMSResponse);

        }

        #region Agent Details
        [HttpPost("AgentList")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> AgetList()
        {
            HmsResponse hMSResponse = new HmsResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            List<AgentDto> agents = new List<AgentDto>();
            var agent = await _context.Agents.ToListAsync();

            if (agent != null)
            {
                agents = _mapper.Map<List<AgentDto>>(agent);
                //agents.Add(agentDto1);
                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                hMSResponse.responseBody.agents = agents;// agent.ToList();
            }
            else
            {
                hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
            }
            return agent == null ? NotFound(hMSResponse) : Ok(hMSResponse);

            //var agent = await _context.Agents.Where(u => u.AgentCode == AgentCode).ToListAsync();
            //var agent = await _context.Agents.ToListAsync();
            //// var agent = await _context.Agents.FindAsync(userid);
            //if (agent == null)
            //    return NotFound();

            //return Ok(agent); ;
        }
        [HttpPost("AgentByCode")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetAgentByCode(SearchAgent agentDto)
        {
            HmsResponse hMSResponse = new HmsResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(agentDto.AgentCode))
            {
                hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                return NotFound(hMSResponse);
            }
            
            List<AgentDto> agents = new List<AgentDto>();
            var AgentId = await _context.Agents
                .Where(u => u.AgentCode.ToUpper() == agentDto.AgentCode.ToUpper())
                .Select(x=> x.AgentId)
                .FirstOrDefaultAsync();
            agentDto.AgentId = AgentId;
            return await GetAgentById(agentDto);
        }
        #endregion
        #region Agent save details

        [HttpPost("SaveAgentlicense")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> SaveAgentlicense([FromBody] DtoAgentLicense agentLicense)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AgnetLicenseResponse hMSResponse = new AgnetLicenseResponse();
            var agentDtos = await _db.ExecuteQueryAsync<DtoAgentLicenseRes>(
              "Agent",
              "SaveAgentlicense",
              new
              {
                  p_agent_id = agentLicense.AgentId,
                  p_license_no = agentLicense.LicenseNo,
                  p_license_type_code = agentLicense.LicenseTypeCode,
                  p_effective_from_date = agentLicense.EffectiveFromDate,// DateTime.Today.ToString("yyyy/MM/dd"),
                  p_created_by = agentLicense.CreatedBy,
                  p_effective_to_date = agentLicense.EffectiveToDate,// DateTime.Today.ToString("yyyy/MM/dd"),
                  p_license_status = agentLicense.LicenseStatus,
                  p_modified_by = agentLicense.ModifiedBy,
                  p_rowversion = agentLicense.RowVersion
              });

            if (agentDtos != null)
            {
                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                hMSResponse.responseBody.agnetLicense = agentDtos.ToList();
            }
            else
            {
                hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
            }
            return agentDtos == null ? NotFound(hMSResponse) : Ok(hMSResponse);

        }

        #endregion
    }
}