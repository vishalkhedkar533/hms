using AutoMapper;
using CommonLibrary;
using CommonLibrary.Background;
using HMS.Caching;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.Enums;
using Models.HMSConsts;
using Mono.TextTemplating;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics.Metrics;

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
        private readonly IAuthClaimService _authClaimService;
        private readonly GenericCacheService _cacheService;
        private int refreshInterval = 15;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Agent> _logger;
        public AgentController(HMSContext context, IConfiguration config, IMapper mapper, IConfiguration configuration
            , DatabaseService db, IAuthClaimService authClaimService, GenericCacheService cacheService, ILogger<Agent> logger)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _db = db;
            _authClaimService = authClaimService;
            _cacheService = cacheService;
            _configuration = configuration;
            int refreshInterval = _configuration.GetValue<int>("Caching:refreshIntervalMinutes", 15);
            _logger = logger;
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

        private List<KeyValueEntry> GetMasterData(string EntryCategory)
        {
            var masterTableConfigs = (_cacheService.GetRecordsAsync<MasterTable>(
                    "hmsmaster",
                    "mastertables",
                    Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"),
                    $" AND EntryCategory = '{EntryCategory}'",
                    refreshInterval)).Result.FirstOrDefault();

            var result = (_cacheService.GetRecordsAsync<KeyValueEntry>(
                masterTableConfigs?.SchemaName
                , masterTableConfigs?.TableName
                , Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0")
                , (masterTableConfigs?.FilterCriteria ?? string.Empty)
                , refreshInterval)).Result.ToList<KeyValueEntry>();
            return result;
        }

        [HttpPost("AgentById")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetAgentById(SearchAgent searchAgent)
        {
            HmsResponse hMSResponse = new HmsResponse();
            Console.WriteLine(_authClaimService.GetClaim(ApiConstants.OrganisationId) );
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            List<AgentDto> agents = new List<AgentDto>();
            AgentDto agentDTO = new AgentDto();
            IQueryable<Agent> agent = _context.Agents;

            #region FetchLoggedInUserInfo
            //string jwtString = "";

            //var decoder = new JwtDecoder();
            //var claimsData = decoder.GetClaimsFromJwt(jwtString);

            //Console.WriteLine("--- Decoded JWT Claims ---");
            //foreach (var kvp in claimsData)
            //{
            //    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            //}

            //// Example of retrieving a specific claim
            //string userId = decoder.GetSpecificClaim(jwtString, "sub");
            //Console.WriteLine($"\nUser ID (sub): {userId}");
            #endregion

            if (searchAgent.AgentId == null)
            {
                hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                return NotFound(hMSResponse);
            }
            var agentEntity = await agent.Where(x => x.AgentId == searchAgent.AgentId 
            && x.OrgId == Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"))
                .AsNoTracking().FirstOrDefaultAsync();
            if (agentEntity != null)
            {
                agentDTO = _mapper.Map<AgentDto>(agentEntity);
                #region getSupervisors
                var stringResponse = await _db.ExecuteQueryAsync<string>(
                    "Agent",
                    "get_agent_supervisors",
                    new
                    {
                        p_agent_id = searchAgent.AgentId
                    });

                if (!string.IsNullOrEmpty(stringResponse.FirstOrDefault()))
                {
                    List<PeopleHeirarchyDto> agentHeirarchyDtos = JsonConvert.DeserializeObject<List<PeopleHeirarchyDto>>(
                        stringResponse.FirstOrDefault(),
                        new Newtonsoft.Json.JsonSerializerSettings
                        {
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                            MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                    agentDTO.peopleHeirarchy = agentHeirarchyDtos;
                }
                #endregion getSupervisors
                if (searchAgent.FetchHierarchy)
                {
                    var immediateSupervisors = await _db.ExecuteQueryAsync<AgentDto>(
                        "Agent",
                        "get_immediate_supervisors",
                        new
                        {
                            p_agent_id = searchAgent.AgentId,
                            p_orgid = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0")
                        });

                    var immediateReportees = await _db.ExecuteQueryAsync<AgentDto>(
                        "Agent",
                        "get_immediate_reportees",
                        new
                        {
                            p_agent_id = searchAgent.AgentId,
                            p_orgid = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0")
                        });

                    List<AgentDto> supervisorsDTO = _mapper.Map<List<AgentDto>>(immediateSupervisors);
                    List<AgentDto> reporteesDTO = _mapper.Map<List<AgentDto>>(immediateReportees);

                    agentDTO.Supervisors = supervisorsDTO;
                    agentDTO.Reportees = reporteesDTO;
                }
                var auditTrail = await _context.AgentAuditTrail
                    .Where(a => a.AgentId == searchAgent.AgentId)
                    .AsNoTracking()
                    .ToListAsync();

                agentDTO.bankAccounts = await _context.BankAccount
                    .Where(b => agentEntity.AgentId == b.RefKey && Models.Enums.ReferenceType.Agent == b.RefType)
                    .AsNoTracking()
                    .ToListAsync();

                //Permanent Address
                agentDTO.PermanentAddres = await _context.Address
                .Where(b => agentEntity.AgentId == b.RefKey && Models.Enums.ReferenceType.Agent == b.RefType && Models.Enums.AddressType.Permanent == b.AddressType)
                .AsNoTracking()
                .ToListAsync();

                //Mailing Address
                agentDTO.MailingAddres = await _context.Address
                    .Where(b => agentEntity.AgentId == b.RefKey && Models.Enums.ReferenceType.Agent == b.RefType && Models.Enums.AddressType.Correspondence == b.AddressType)
                    .AsNoTracking()
                    .ToListAsync();

                //Nominees
                agentDTO.nominees = await _context.Nominee
                    .Where(b => agentEntity.AgentId == b.RefKey && Models.Enums.ReferenceType.Agent == b.RefType)
                    .AsNoTracking()
                    .ToListAsync();

                //Personal Infomation
                agentDTO.personalInfo = await _context.PersonalInfo
                    .Where(b => agentEntity.AgentId == b.RefKey && Models.Enums.ReferenceType.Agent == b.RefType)
                    .AsNoTracking()
                    .ToListAsync();

                var AgentProfileMst = GetMasterData("AgentProfileMst").ToList();
                var BankAccType = AgentProfileMst.Where(x=> x.EntryCategory == "BANK_ACC_TYP");
                var AgentClass = AgentProfileMst.Where(x => x.EntryCategory == "AGENT_CLASS");
                var SalesSubChannels = AgentProfileMst.Where(x => x.EntryCategory == "SUB_CHANNEL");
                var State = AgentProfileMst.Where(x => x.EntryCategory == "STATE_NAME");
                var Occupations = AgentProfileMst.Where(x => x.EntryCategory == "OCCUPATION");
                var MaritalStatus = AgentProfileMst.Where(x => x.EntryCategory == "MARITAL_STATUS");
                var Gender = AgentProfileMst.Where(x => x.EntryCategory == "GENDER");
                var EducationQualification = AgentProfileMst.Where(x => x.EntryCategory == "EDUCATION_CODE");
                var Country = AgentProfileMst.Where(x => x.EntryCategory == "COUNTRY");
                var SalesChannels = AgentProfileMst.Where(x => x.EntryCategory == "CHANNEL_NAME");
                var AgentTypeCategory = AgentProfileMst.Where(x => x.EntryCategory == "AGENT_TYPE_CAT");
                var Salutation = AgentProfileMst.Where(x => x.EntryCategory == "TITLE");
                var AgentType = AgentProfileMst.Where(x => x.EntryCategory == "AGNT_TYP");
                var CommissionClass = AgentProfileMst.Where(x => x.EntryCategory == "COMMISSION_CLASS");
                var CandidateType = AgentProfileMst.Where(x => x.EntryCategory == "CANDIDATE_TYP");

                foreach (var bankAcc in agentDTO.bankAccounts)
                {
                    bankAcc.AccountTypeDesc = BankAccType
                        .Where(b => b.EntryIdentity == bankAcc.AccountType)
                        .Select(b => b.EntryDesc)
                        .FirstOrDefault() ?? string.Empty;
                }

                agentDTO.AgentClassDesc = AgentClass.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.AgentClass ??  -1000))?.EntryDesc ?? string.Empty;
                agentDTO.SubChannelDesc = SalesSubChannels.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.SubChannel ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.StateDesc = State.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.State ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.OccupationDesc = Occupations.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Occupation ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.MaritalStatusDesc = MaritalStatus.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.MaritalStatus ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.GenderDesc = Gender.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Gender ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.EducationDesc = EducationQualification.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Education ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.CountryDesc = Country.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Country ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.ChannelDesc = SalesChannels.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Channel ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.AgentTypeCodeDesc = AgentTypeCategory.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.AgentTypeCode ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.TitleDesc = Salutation.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Title ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.AgentTypeDesc = AgentType.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.AgentType ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.CommissionClassDesc = CommissionClass.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.CommissionClass ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.CandidateTypeDesc = CandidateType.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.CandidateType ?? -1000))?.EntryDesc?? string.Empty;

                List<AgentAuditTrailDTO> agentAuditTrailDTOs = _mapper.Map<List<AgentAuditTrailDTO>>(auditTrail);
                agentDTO.agentAuditTrail = agentAuditTrailDTOs;
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
                .Select(x => x.AgentId)
                .FirstOrDefaultAsync();
            agentDto.AgentId = AgentId;
            agentDto.FetchHierarchy = true;
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
        [HttpPost("Bulk/Create")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> BulkAgentCreate(SearchAgent searchAgent,
            [FromServices] IExcelProcessingQueue queue)
        {
            HmsResponse hMSResponse = new HmsResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //return agentEntity == null ? NotFound(hMSResponse) : Ok(hMSResponse);
            return Ok(hMSResponse);
        }

        [HttpPost("Update/{id}")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UpdateAgent(int id, [FromBody] AgentDto agentDto)
        {
            // 1. Basic Validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != agentDto.AgentId)
                return BadRequest("ID mismatch between URL and body.");

            var username = HttpContext?.User?.Identity?.Name ?? "System";
            var orgId = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            // 2. Fetch existing agent (with related data if needed for auditing)
            var existingAgent = await _context.Agents
                .FirstOrDefaultAsync(a => a.AgentId == id && a.OrgId == orgId);

            if (existingAgent == null)
                return NotFound(new { message = $"Agent with ID {id} not found for this organization." });
            // 4. Map DTO to existing Entity (Syncs changes)
            _mapper.Map(agentDto, existingAgent);

            // 5. Update Metadata
            existingAgent.ModifiedBy = username;
            existingAgent.ModifiedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

                // 6. Return updated DTO
                var updatedDto = _mapper.Map<AgentDto>(existingAgent);
                return Ok(new { message = "Agent updated successfully", data = updatedDto });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The record was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent {AgentId}", id);
                return StatusCode(500, "Internal server error during update.");
            }
        }

    }
}