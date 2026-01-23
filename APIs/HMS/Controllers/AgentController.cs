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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        [HttpPost("UpdateAgent/{id}/{sectionName}")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UpdateAgent(
            [FromRoute] int id,
            [FromRoute] string sectionName,
            [FromBody] AgentDto agentDto)
        {
            ModelState.Clear();
            HmsResponse hmsResponse = new HmsResponse();
            var username = HttpContext?.User?.Identity?.Name ?? "System";
            var orgId = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            try
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.AgentId == id && a.OrgId == orgId);

                if (agent == null)
                {
                    hmsResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                    hmsResponse.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";
                    return NoContent();
                }

                var updatedFields = new List<UpdatedAgentField>();

                // Helper to record changes
                void RecordChange(string field, object? oldVal, object? newVal)
                {
                    var oldS = oldVal == null ? string.Empty : (oldVal is DateTime odt ? odt.ToString("o") : oldVal.ToString());
                    var newS = newVal == null ? string.Empty : (newVal is DateTime ndt ? ndt.ToString("o") : newVal.ToString());
                    if (oldS != newS)
                    {
                        updatedFields.Add(new UpdatedAgentField { FieldName = field, OldValue = oldS ?? string.Empty, NewValue = newS ?? string.Empty });
                    }
                }

                // Snapshot of scalar fields to compare after modification
                var snapshot = new Dictionary<string, object?>()
                {
                    { "Title", agent.Title },
                    { "FirstName", agent.FirstName },
                    { "MiddleName", agent.MiddleName },
                    { "LastName", agent.LastName },
                    { "FatherHusbandNm", agent.FatherHusbandNm },
                    { "Gender", agent.Gender },
                    { "Dob", agent.Dob },
                    { "MaritalStatus", agent.MaritalStatus },
                    { "Nationality", agent.Nationality },
                    { "PreferredLanguage", agent.PreferredLanguage },
                    { "Email", agent.Email },
                    { "MobileNo", agent.MobileNo },
                    { "CnctPersonName", agent.CnctPersonName },
                    { "CnctPersonMobileNo", agent.CnctPersonMobileNo },
                    { "CnctPersonEmail", agent.CnctPersonEmail },
                    { "CnctPersonDesig", agent.CnctPersonDesig },
                    { "AgentCode", agent.AgentCode },
                    { "AgentType", agent.AgentType },
                    { "AgentTypeCode", agent.AgentTypeCode },
                    { "AgentSubTypeCode", agent.AgentSubTypeCode },
                    { "AgentClass", agent.AgentClass },
                    { "AgentLevel", agent.AgentLevel },
                    { "StaffCode", agent.StaffCode },
                    { "SupervisorId", agent.SupervisorId },
                    { "LicenseNo", agent.LicenseNo },
                    { "LicenseType", agent.LicenseType },
                    { "LicenseIssueDate", agent.LicenseIssueDate },
                    { "LicenseExpiryDate", agent.LicenseExpiryDate },
                    { "LicenseStatus", agent.LicenseStatus },
                    { "IsLicensed", agent.IsLicensed },
                    { "PanAadharLinkFlag", agent.PanAadharLinkFlag },
                    { "Sec206abFlag", agent.Sec206abFlag },
                    { "TaxStatus", agent.TaxStatus },
                    { "ServiceTaxNo", agent.ServiceTaxNo },
                    { "MainPartnerClientCode", agent.MainPartnerClientCode },
                    { "ApplicationDocketNo", agent.ApplicationDocketNo },
                    { "EmployeeCode", agent.EmployeeCode },
                    { "StartDate", agent.StartDate },
                    { "AppointmentDate", agent.AppointmentDate },
                    { "IncorporationDate", agent.IncorporationDate },
                    { "AgentTypeCategory", agent.AgentTypeCategory },
                    { "AgentClassification", agent.AgentClassification },
                    { "CmsAgentType", agent.CmsAgentType },
                    { "CommissionClass", agent.CommissionClass },
                    { "BankAccType", agent.BankAccType },
                    { "UlipFlag", agent.UlipFlag },
                    { "IsMigrated", agent.IsMigrated },
                    { "AgentMaincodeVweid", agent.AgentMaincodeVweid },
                    { "RegistrationDate", agent.RegistrationDate },
                    { "Vertical", agent.Vertical },
                    { "TrainingGroupType", agent.TrainingGroupType },
                    { "RefresherTrainingCompleted", agent.RefresherTrainingCompleted },
                    { "Education", agent.Education },
                    { "Occupation", agent.Occupation },
                    { "Urn", agent.Urn },
                    { "AdditionalComment", agent.AdditionalComment }
                };

                 switch ((sectionName ?? string.Empty).ToLowerInvariant())
                  {
                      case "individual_agent_action":
                        if (agentDto.Channel.HasValue)
                            agent.Channel = agentDto.Channel; 
                        if (agentDto.SubChannel.HasValue)
                            agent.SubChannel = agentDto.SubChannel; 
                        if (agentDto.AgentClass.HasValue)
                            agent.AgentClass = agentDto.AgentClass; 
                        
                        break;

                      case "personal_details":
                    if (agentDto.Title.HasValue)
                        agent.Title = agentDto.Title;

                    if (!string.IsNullOrWhiteSpace(agentDto.FirstName))
                        agent.FirstName = agentDto.FirstName;

                    if (!string.IsNullOrWhiteSpace(agentDto.MiddleName))
                        agent.MiddleName = agentDto.MiddleName;

                    if (!string.IsNullOrWhiteSpace(agentDto.LastName))
                        agent.LastName = agentDto.LastName;

                    if (!string.IsNullOrWhiteSpace(agentDto.Father_Husband_Nm))
                        agent.FatherHusbandNm = agentDto.Father_Husband_Nm;

                    if (agentDto.Gender.HasValue)
                        agent.Gender = agentDto.Gender;

                    if (agentDto.DOB.HasValue)
                        agent.Dob = agentDto.DOB;

                    //if (agentDto.MaritalStatus.HasValue)
                    //    agent.MaritalStatus = agentDto.MaritalStatus;

                    //if (!string.IsNullOrWhiteSpace(agentDto.Nationality))
                    //    agent.Nationality = agentDto.Nationality;

                    //if (!string.IsNullOrWhiteSpace(agentDto.PreferredLanguage))
                    //    agent.PreferredLanguage = agentDto.PreferredLanguage;

                    break;

                      case "contact_information":
                        
                    if (!string.IsNullOrWhiteSpace(agentDto.MobileNo))
                        agent.MobileNo = agentDto.MobileNo;

                    if (!string.IsNullOrWhiteSpace(agentDto.Email))
                        agent.Email = agentDto.Email;

                    // Update contact person fields
                    if (!string.IsNullOrWhiteSpace(agentDto.CnctPersonName))
                        agent.CnctPersonName = agentDto.CnctPersonName;
                    if (!string.IsNullOrWhiteSpace(agentDto.CnctPersonMobileNo))
                        agent.CnctPersonMobileNo = agentDto.CnctPersonMobileNo;
                    if (!string.IsNullOrWhiteSpace(agentDto.CnctPersonEmail))
                        agent.CnctPersonEmail = agentDto.CnctPersonEmail;
                    if (!string.IsNullOrWhiteSpace(agentDto.CnctPersonDesig))
                        agent.CnctPersonDesig = agentDto.CnctPersonDesig;

                        // Upsert work/residence contact numbers in PersonalInfo
                        if (agentDto.personalInfo != null && agentDto.personalInfo.Any())
                        {
                            var p = agentDto.personalInfo.First();
                            var existingPI = await _context.PersonalInfo
                                .FirstOrDefaultAsync(x => x.RefKey == agent.AgentId && x.RefType == ReferenceType.Agent);
                            if (existingPI != null)
                            {
                                var oldWork = existingPI.WorkContactNo ?? string.Empty;
                                var oldRes = existingPI.ResidenceContactNo ?? string.Empty;
                                var newWork = p.WorkContactNo ?? string.Empty;
                                var newRes = p.ResidenceContactNo ?? string.Empty;

                                if (!string.Equals(oldWork, newWork, StringComparison.Ordinal))
                                {
                                    existingPI.WorkContactNo = p.WorkContactNo ?? existingPI.WorkContactNo;
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "WorkContactNo", OldValue = oldWork, NewValue = newWork });
                                }

                                if (!string.Equals(oldRes, newRes, StringComparison.Ordinal))
                                {
                                    existingPI.ResidenceContactNo = p.ResidenceContactNo ?? existingPI.ResidenceContactNo;
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "ResidenceContactNo", OldValue = oldRes, NewValue = newRes });
                                }

                                // If DTO includes Email/Mobile in personalInfo, keep PersonalInfo in sync
                                if (!string.IsNullOrWhiteSpace(p.Email))
                                    existingPI.Email = p.Email;
                                if (!string.IsNullOrWhiteSpace(p.MobileNo))
                                    existingPI.MobileNo = p.MobileNo;

                                _context.PersonalInfo.Update(existingPI);
                            }
                            else
                            {
                                // create a new PersonalInfo only for contact numbers if provided
                                if (!string.IsNullOrWhiteSpace(p.WorkContactNo) || !string.IsNullOrWhiteSpace(p.ResidenceContactNo)
                                    || !string.IsNullOrWhiteSpace(p.Email) || !string.IsNullOrWhiteSpace(p.MobileNo))
                                {
                                    var newPI = new PersonalInfo
                                    {
                                        RefKey = agent.AgentId,
                                        RefType = ReferenceType.Agent,
                                        WorkContactNo = p.WorkContactNo ?? string.Empty,
                                        ResidenceContactNo = p.ResidenceContactNo ?? string.Empty,
                                        Email = p.Email,
                                        MobileNo = p.MobileNo,
                                        DateOfBirth = p.DateOfBirth == default ? existingPI?.DateOfBirth ?? default : p.DateOfBirth
                                    };
                                    await _context.PersonalInfo.AddAsync(newPI);

                                    updatedFields.Add(new UpdatedAgentField { FieldName = "WorkContactNo", OldValue = string.Empty, NewValue = p.WorkContactNo ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "ResidenceContactNo", OldValue = string.Empty, NewValue = p.ResidenceContactNo ?? string.Empty });
                                }
                            }
                        }

                    break;

                      case "employee_info":
                    if (!string.IsNullOrWhiteSpace(agentDto.ApplicationDocketNo))
                        agent.ApplicationDocketNo = agentDto.ApplicationDocketNo;

                    if (agentDto.CandidateType.HasValue)
                        agent.CandidateType = agentDto.CandidateType;

                    if (agentDto.AgentType.HasValue)
                        agent.AgentType = agentDto.AgentType;

                    if (!string.IsNullOrWhiteSpace(agentDto.EmployeeCode))
                        agent.EmployeeCode = agentDto.EmployeeCode;

                    if (agentDto.StartDate.HasValue)
                        agent.StartDate = agentDto.StartDate;

                    if (agentDto.AppointmentDate.HasValue)
                        agent.AppointmentDate = agentDto.AppointmentDate;

                    if (agentDto.IncorporationDate.HasValue)
                        agent.IncorporationDate = agentDto.IncorporationDate;

                    if (!string.IsNullOrWhiteSpace(agentDto.AgentTypeCategory))
                        agent.AgentTypeCat = agentDto.AgentTypeCat;

                    if (!string.IsNullOrWhiteSpace(agentDto.AgentClassification))
                        agent.AgentClass = agentDto.AgentClass;

                    if (!string.IsNullOrWhiteSpace(agentDto.CMSAgentType))
                        agent.CmsAgentType = agentDto.CMSAgentType;

                    break;

                      case "financial_details":
                          agent.PanAadharLinkFlag = agentDto.PanAadharLinkFlag;
                          agent.Sec206abFlag = agentDto.Sec206abFlag;
                          if (!string.IsNullOrWhiteSpace(agentDto.TaxStatus))
                              agent.TaxStatus = agentDto.TaxStatus;
                          if (!string.IsNullOrWhiteSpace(agentDto.ServiceTaxNo))
                              agent.ServiceTaxNo = agentDto.ServiceTaxNo;
                          if (!string.IsNullOrWhiteSpace(agentDto.MaskedPanNumber))
                              agent.PanNumber = agentDto.MaskedPanNumber;


                        if (agentDto.bankAccounts != null && agentDto.bankAccounts.Any())
                        {
                            // record that bank details section updated
                            updatedFields.Add(new UpdatedAgentField { FieldName = "financial_details", OldValue = string.Empty, NewValue = "updated" });

                            var b = agentDto.bankAccounts.First();
                            var existingBank = await _context.BankAccount
                                .FirstOrDefaultAsync(x => x.RefKey == agent.AgentId && x.RefType == ReferenceType.Agent);
                            if (existingBank != null)


                            {
                                // capture old values
                                var oldAccountHolder = existingBank.AccountHolderName ?? string.Empty;
                                var oldAccountNumber = existingBank.AccountNumber ?? string.Empty;
                                var oldIFSC = existingBank.IFSC ?? string.Empty;
                                var oldMICR = existingBank.MICR ?? string.Empty;
                                var oldBankName = existingBank.BankName ?? string.Empty;
                                var oldBranchName = existingBank.BranchName ?? string.Empty;
                                var oldAccountType = existingBank.AccountType.ToString();
                                var oldActiveSince = existingBank.ActiveSince?.ToString("o") ?? string.Empty;
                                var oldFactoringHouse = existingBank.FactoringHouse ?? string.Empty;
                                var oldPrefPayment = existingBank.PreferredPaymentMode.ToString();

                                // apply updates
                                existingBank.AccountHolderName = b.AccountHolderName ?? existingBank.AccountHolderName;
                                existingBank.AccountNumber = b.AccountNumber ?? existingBank.AccountNumber;
                                existingBank.IFSC = b.IFSC ?? existingBank.IFSC;
                                existingBank.MICR = b.MICR ?? existingBank.MICR;
                                existingBank.BankName = b.BankName ?? existingBank.BankName;
                                existingBank.BranchName = b.BranchName ?? existingBank.BranchName;
                                existingBank.AccountType = b.AccountType != 0 ? b.AccountType : existingBank.AccountType;
                                existingBank.ActiveSince = b.ActiveSince ?? existingBank.ActiveSince;
                                existingBank.FactoringHouse = b.FactoringHouse ?? existingBank.FactoringHouse;
                                existingBank.PreferredPaymentMode = b.PreferredPaymentMode;
                                _context.BankAccount.Update(existingBank);

                                // record field-level changes for bank account
                                void AddIfChanged(string fieldName, string oldV, string newV)
                                {
                                    if ((oldV ?? string.Empty) != (newV ?? string.Empty))
                                    {
                                        updatedFields.Add(new UpdatedAgentField { FieldName = fieldName, OldValue = oldV ?? string.Empty, NewValue = newV ?? string.Empty });
                                    }
                                }

                                AddIfChanged("AccountHolderName", oldAccountHolder, existingBank.AccountHolderName);
                                AddIfChanged("AccountNumber", oldAccountNumber, existingBank.AccountNumber);
                                AddIfChanged("IFSC", oldIFSC, existingBank.IFSC);
                                AddIfChanged("MICR", oldMICR, existingBank.MICR);
                                AddIfChanged("BankName", oldBankName, existingBank.BankName);
                                AddIfChanged("BranchName", oldBranchName, existingBank.BranchName);
                                AddIfChanged("AccountType", oldAccountType, existingBank.AccountType.ToString());
                                AddIfChanged("ActiveSince", oldActiveSince, existingBank.ActiveSince?.ToString("o") ?? string.Empty);
                                AddIfChanged("FactoringHouse", oldFactoringHouse, existingBank.FactoringHouse);
                                AddIfChanged("PreferredPaymentMode", oldPrefPayment, existingBank.PreferredPaymentMode.ToString());
                            }
                            else
                            {
                                var newBank = new BankAccount
                                {
                                    RefKey = agent.AgentId,
                                    RefType = ReferenceType.Agent,
                                    AccountHolderName = b.AccountHolderName ?? (agent.FirstName + " " + agent.LastName)?.Trim(),
                                    AccountNumber = b.AccountNumber ?? string.Empty,
                                    IFSC = b.IFSC ?? string.Empty,
                                    MICR = b.MICR,
                                    BankName = b.BankName,
                                    BranchName = b.BranchName,
                                    AccountType = b.AccountType != 0 ? b.AccountType : 1,
                                    ActiveSince = b.ActiveSince ?? DateTime.Now,
                                    FactoringHouse = b.FactoringHouse,
                                    PreferredPaymentMode = b.PreferredPaymentMode
                                };
                                await _context.BankAccount.AddAsync(newBank);

                                // record created bank account fields
                                updatedFields.Add(new UpdatedAgentField { FieldName = "AccountHolderName", OldValue = string.Empty, NewValue = newBank.AccountHolderName ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "AccountNumber", OldValue = string.Empty, NewValue = newBank.AccountNumber ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "IFSC", OldValue = string.Empty, NewValue = newBank.IFSC ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "MICR", OldValue = string.Empty, NewValue = newBank.MICR ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "BankName", OldValue = string.Empty, NewValue = newBank.BankName ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "BranchName", OldValue = string.Empty, NewValue = newBank.BranchName ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "AccountType", OldValue = string.Empty, NewValue = newBank.AccountType.ToString() });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "ActiveSince", OldValue = string.Empty, NewValue = newBank.ActiveSince?.ToString("o") ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "FactoringHouse", OldValue = string.Empty, NewValue = newBank.FactoringHouse ?? string.Empty });
                                updatedFields.Add(new UpdatedAgentField { FieldName = "PreferredPaymentMode", OldValue = string.Empty, NewValue = newBank.PreferredPaymentMode.ToString() });
                            }
                        }
                        break;

                      case "other_personal_details":
                    {
                        // 1) Agent-level fields come from AgentDto
                        if (agentDto.MaritalStatus.HasValue)
                            agent.MaritalStatus = agentDto.MaritalStatus;

                        if (agentDto.Education.HasValue)
                            agent.Education = agentDto.Education;

                        if (agentDto.Occupation.HasValue)
                            agent.Occupation = agentDto.Occupation;

                        if (!string.IsNullOrWhiteSpace(agentDto.URN))
                            agent.Urn = agentDto.URN;

                        if (!string.IsNullOrWhiteSpace(agentDto.AdditionalComment))
                            agent.AdditionalComment = agentDto.AdditionalComment;

                        // 2) PersonalInfo fields come from agentDto.personalInfo[0]
                        if (agentDto.personalInfo != null && agentDto.personalInfo.Any())
                        {
                            updatedFields.Add(new UpdatedAgentField { FieldName = "personalInfo", OldValue = string.Empty, NewValue = "updated" });

                            var p = agentDto.personalInfo.First();
                            var existingPI = await _context.PersonalInfo
                                .FirstOrDefaultAsync(x => x.RefKey == agent.AgentId && x.RefType == ReferenceType.Agent);

                            if (existingPI != null)
                            {
                                if (p.DateOfBirth != default) existingPI.DateOfBirth = p.DateOfBirth;
                                existingPI.WorkProfile = p.WorkProfile ?? existingPI.WorkProfile;
                                existingPI.AnnualIncome = p.AnnualIncome ?? existingPI.AnnualIncome;
                                existingPI.BloodGroup = p.BloodGroup ?? existingPI.BloodGroup;
                                existingPI.BirthPlace = p.BirthPlace ?? existingPI.BirthPlace;

                                // keep other personalInfo fields in sync
                                existingPI.PanNumber = p.PanNumber ?? existingPI.PanNumber;
                                existingPI.Email = p.Email ?? existingPI.Email;
                                existingPI.MobileNo = p.MobileNo ?? existingPI.MobileNo;
                                existingPI.WorkContactNo = p.WorkContactNo ?? existingPI.WorkContactNo;
                                existingPI.ResidenceContactNo = p.ResidenceContactNo ?? existingPI.ResidenceContactNo;
                                if (p.MartialStatus != null) existingPI.MartialStatus = p.MartialStatus;
                                existingPI.EducationCode = p.EducationCode ?? existingPI.EducationCode;
                                existingPI.EducationLevel = p.EducationLevel ?? existingPI.EducationLevel;
                                existingPI.WorkExpMonths = p.WorkExpMonths ?? existingPI.WorkExpMonths;

                                _context.PersonalInfo.Update(existingPI);
                            }
                            else if (p.DateOfBirth != default)
                            {
                                var newPI = new PersonalInfo
                                {
                                    RefKey = agent.AgentId,
                                    RefType = ReferenceType.Agent,
                                    DateOfBirth = p.DateOfBirth,
                                    WorkProfile = p.WorkProfile,
                                    AnnualIncome = p.AnnualIncome,
                                    BloodGroup = p.BloodGroup,
                                    BirthPlace = p.BirthPlace,
                                    PanNumber = p.PanNumber,
                                    Email = p.Email,
                                    MobileNo = p.MobileNo,
                                    WorkContactNo = p.WorkContactNo,
                                    ResidenceContactNo = p.ResidenceContactNo,
                                    MartialStatus = p.MartialStatus,
                                    EducationCode = p.EducationCode,
                                    EducationLevel = p.EducationLevel,
                                    WorkExpMonths = p.WorkExpMonths
                                };
                                await _context.PersonalInfo.AddAsync(newPI);
                            }
                        }

                        // 3) Nominee fields come from agentDto.nominees[0]
                        if (agentDto.nominees != null && agentDto.nominees.Any())
                        {
                            var n = agentDto.nominees.First();
                            if (n != null)
                            {
                                var existingNom = await _context.Nominee.FirstOrDefaultAsync(x => x.RefKey == agent.AgentId && x.RefType == ReferenceType.Agent);
                                if (existingNom != null)
                                {
                                    existingNom.NomineeName = n.NomineeName ?? existingNom.NomineeName;
                                    existingNom.Relationship = n.Relationship ?? existingNom.Relationship;
                                    existingNom.NomineeAge = n.NomineeAge != 0 ? n.NomineeAge : existingNom.NomineeAge;
                                    existingNom.IsActive = n.IsActive;
                                    _context.Nominee.Update(existingNom);
                                }
                            }
                            else
                            {
                                var newNom = new Nominee
                                {
                                    RefKey = agent.AgentId,
                                    RefType = ReferenceType.Agent,
                                    NomineeName = n.NomineeName,
                                    Relationship = n.Relationship,
                                    PercentageShare = n.PercentageShare,
                                    IsActive = n.IsActive,
                                    NomineeAge = n.NomineeAge
                                };
                                await _context.Nominee.AddAsync(newNom);
                            }
                        }

                        break;
                    }

                    case "address_config":
                        {
                            if (agentDto.PermanentAddres != null && agentDto.PermanentAddres.Any())
                            {
                                var addrDto = agentDto.PermanentAddres.First();

                                var existingAddress = await _context.Address
                                    .FirstOrDefaultAsync(x =>
                                        x.RefKey == agent.AgentId &&
                                        x.RefType == ReferenceType.Agent);

                                if (existingAddress != null)
                                {
                                    existingAddress.AddressLine1 = addrDto.AddressLine1 ?? existingAddress.AddressLine1;
                                    existingAddress.AddressLine2 = addrDto.AddressLine2 ?? existingAddress.AddressLine2;
                                    existingAddress.AddressLine3 = addrDto.AddressLine3 ?? existingAddress.AddressLine3;
                                    existingAddress.City = addrDto.City ?? existingAddress.City;
                                    existingAddress.State = addrDto.State ?? existingAddress.State;
                                    existingAddress.Country = addrDto.Country ?? existingAddress.Country;
                                    existingAddress.PIN = addrDto.PIN ?? existingAddress.PIN;
                                    existingAddress.Landmark = addrDto.Landmark ?? existingAddress.Landmark;

                                    _context.Address.Update(existingAddress);

                                    updatedFields.Add(new UpdatedAgentField
                                    {
                                        FieldName = "PermanentAddress",
                                        OldValue = "existing",
                                        NewValue = "updated"
                                    });
                                }
                                else
                                {
                                    var newAddress = new Address
                                    {
                                        RefKey = agent.AgentId,
                                        RefType = ReferenceType.Agent,
                                        AddressType = addrDto.AddressType,
                                        AddressLine1 = addrDto.AddressLine1,
                                        AddressLine2 = addrDto.AddressLine2,
                                        AddressLine3 = addrDto.AddressLine3,
                                        City = addrDto.City,
                                        State = addrDto.State,
                                        Country = addrDto.Country,
                                        PIN = addrDto.PIN,
                                        Landmark = addrDto.Landmark
                                    };

                                    await _context.Address.AddAsync(newAddress);

                                    updatedFields.Add(new UpdatedAgentField
                                    {
                                        FieldName = "PermanentAddress",
                                        OldValue = string.Empty,
                                        NewValue = "created"
                                    });
                                }
                            }

                            break;
                        }


                    case "official_details":
                          if (!string.IsNullOrWhiteSpace(agentDto.AgentCode))
                              agent.AgentCode = agentDto.AgentCode;

                          if (agentDto.AgentType.HasValue)
                              agent.AgentType = agentDto.AgentType;

                          if (agentDto.AgentTypeCode.HasValue)
                              agent.AgentTypeCode = agentDto.AgentTypeCode;

                          if (agentDto.AgentSubTypeCode.HasValue)
                              agent.AgentSubTypeCode = agentDto.AgentSubTypeCode;

                          if (agentDto.AgentClass.HasValue)
                              agent.AgentClass = agentDto.AgentClass;

                          if (!string.IsNullOrWhiteSpace(agentDto.AgentLevel))
                              agent.AgentLevel = agentDto.AgentLevel;

                          if (!string.IsNullOrWhiteSpace(agentDto.StaffCode))
                              agent.StaffCode = agentDto.StaffCode;

                          if (agentDto.SupervisorId.HasValue)
                              agent.SupervisorId = agentDto.SupervisorId;

                          break;

                      case "license_details":
                    if (!string.IsNullOrWhiteSpace(agentDto.LicenseNo))
                        agent.LicenseNo = agentDto.LicenseNo;

                    if (!string.IsNullOrWhiteSpace(agentDto.LicenseType))
                        agent.LicenseType = agentDto.LicenseType;

                    if (agentDto.LicenseIssueDate.HasValue)
                        agent.LicenseIssueDate = agentDto.LicenseIssueDate;

                    if (agentDto.LicenseExpiryDate.HasValue)
                        agent.LicenseExpiryDate = agentDto.LicenseExpiryDate;

                    if (!string.IsNullOrWhiteSpace(agentDto.LicenseStatus))
                        agent.LicenseStatus = agentDto.LicenseStatus;

                    // keep boolean in sync if provided
                    agent.IsLicensed = agentDto.IsLicensed;

                    break;

                      case "bank_details":
                          // Update Agent.BankAccType if provided
                          if (agentDto.BankAccType.HasValue)
                              agent.BankAccType = agentDto.BankAccType;

                          // Upsert bank account record if bank account data provided
                          if (agentDto.bankAccounts != null && agentDto.bankAccounts.Any())
                          {
                              // record that bank details section updated
                              updatedFields.Add(new UpdatedAgentField { FieldName = "bank_details", OldValue = string.Empty, NewValue = "updated" });

                              var b = agentDto.bankAccounts.First();
                              var existingBank = await _context.BankAccount
                                  .FirstOrDefaultAsync(x => x.RefKey == agent.AgentId && x.RefType == ReferenceType.Agent);
                              if (existingBank != null)
                              {
                                  existingBank.AccountHolderName = b.AccountHolderName ?? existingBank.AccountHolderName;
                                  existingBank.AccountNumber = b.AccountNumber ?? existingBank.AccountNumber;
                                  existingBank.IFSC = b.IFSC ?? existingBank.IFSC;
                                  existingBank.MICR = b.MICR ?? existingBank.MICR;
                                  existingBank.BankName = b.BankName ?? existingBank.BankName;
                                  existingBank.BranchName = b.BranchName ?? existingBank.BranchName;
                                  existingBank.AccountType = b.AccountType != 0 ? b.AccountType : existingBank.AccountType;
                                  existingBank.ActiveSince = b.ActiveSince ?? existingBank.ActiveSince;
                                  existingBank.FactoringHouse = b.FactoringHouse ?? existingBank.FactoringHouse;
                                  existingBank.PreferredPaymentMode = b.PreferredPaymentMode;
                                  _context.BankAccount.Update(existingBank);
                              }
                              else
                              {
                                  var newBank = new BankAccount
                                  {
                                      RefKey = agent.AgentId,
                                      RefType = ReferenceType.Agent,
                                      AccountHolderName = b.AccountHolderName ?? (agent.FirstName + " " + agent.LastName)?.Trim(),
                                      AccountNumber = b.AccountNumber ?? string.Empty,
                                      IFSC = b.IFSC ?? string.Empty,
                                      MICR = b.MICR,
                                      BankName = b.BankName,
                                      BranchName = b.BranchName,
                                      AccountType = b.AccountType != 0 ? b.AccountType : 1,
                                      ActiveSince = b.ActiveSince ?? DateTime.Now,
                                      FactoringHouse = b.FactoringHouse,
                                      PreferredPaymentMode = b.PreferredPaymentMode
                                  };
                                  await _context.BankAccount.AddAsync(newBank);
                              }
                          }

                          break;

                      case "product_details":
                    // Ulip and product related flags
                    agent.UlipFlag = agentDto.UlipFlag;
                    break;

                      case "others_details":
                    // Miscellaneous fields
                    agent.IsMigrated = agentDto.IsMigrated;
                    if (!string.IsNullOrWhiteSpace(agentDto.MainPartnerClientCode))
                        agent.MainPartnerClientCode = agentDto.MainPartnerClientCode;
                    if (!string.IsNullOrWhiteSpace(agentDto.AgentMaincodevwEid))
                        agent.AgentMaincodeVweid = agentDto.AgentMaincodevwEid;
                    if (agentDto.RegistrationDate.HasValue)
                        agent.RegistrationDate = agentDto.RegistrationDate;
                    if (!string.IsNullOrWhiteSpace(agentDto.Vertical))
                        agent.Vertical = agentDto.Vertical;
                    break;

                      case "training_details":
                          if (!string.IsNullOrWhiteSpace(agentDto.TrainingGroupType))
                              agent.TrainingGroupType = agentDto.TrainingGroupType;
                          agent.RefresherTrainingCompleted = agentDto.RefresherTrainingCompleted;
                          break;

                      case "nominees":
                    if (agentDto.nominees != null && agentDto.nominees.Any())
                    {
                        // record that nominees section updated
                        updatedFields.Add(new UpdatedAgentField { FieldName = "nominees", OldValue = string.Empty, NewValue = "updated" });

                        foreach (var n in agentDto.nominees)
                        {
                            if (n.NomineeID != 0)
                            {
                                var existingNom = await _context.Nominee.FirstOrDefaultAsync(x => x.NomineeID == n.NomineeID && x.RefKey == agent.AgentId);
                                if (existingNom != null)
                                {
                                    existingNom.NomineeName = n.NomineeName ?? existingNom.NomineeName;
                                    existingNom.Relationship = n.Relationship ?? existingNom.Relationship;
                                    existingNom.PercentageShare = n.PercentageShare != 0 ? n.PercentageShare : existingNom.PercentageShare;
                                    existingNom.IsActive = n.IsActive;
                                    existingNom.NomineeAge = n.NomineeAge != 0 ? n.NomineeAge : existingNom.NomineeAge;
                                    _context.Nominee.Update(existingNom);
                                }
                            }
                            else
                            {
                                var newNom = new Nominee
                                {
                                    RefKey = agent.AgentId,
                                    RefType = ReferenceType.Agent,
                                    NomineeName = n.NomineeName,
                                    Relationship = n.Relationship,
                                    PercentageShare = n.PercentageShare,
                                    IsActive = n.IsActive,
                                    NomineeAge = n.NomineeAge
                                };
                                await _context.Nominee.AddAsync(newNom);
                            }
                        }
                    }
                    break;

                      case "branch_details":
                    if (!string.IsNullOrWhiteSpace(agentDto.BranchCode))
                        agent.BranchCode = agentDto.BranchCode;
                    if (!string.IsNullOrWhiteSpace(agentDto.BranchName))
                        agent.BranchName = agentDto.BranchName;
                    break;

                      case "organisation_details":
                    if (agentDto.ConfirmationDate.HasValue)
                        agent.ConfirmationDate = agentDto.ConfirmationDate;
                    if (agentDto.IncrementDate.HasValue)
                        agent.IncrementDate = agentDto.IncrementDate;
                    if (agentDto.LastPromotionDate.HasValue)
                        agent.LastPromotionDate = agentDto.LastPromotionDate;
                    if (agentDto.HRDoj.HasValue)
                        agent.HrDoj = agentDto.HRDoj;
                    if (agentDto.LastWorkingDate.HasValue)
                        agent.LastWorkingDate = agentDto.LastWorkingDate;
                    break;

                      case "other_training":
                    if (agentDto.Ic36TrngCompletionDate.HasValue)
                        agent.Ic36TrngCompletionDate = agentDto.Ic36TrngCompletionDate;
                    if (agentDto.STrngCompletionDate.HasValue)
                        agent.STrngCompletionDate = agentDto.STrngCompletionDate;
                    if (agentDto.FgRockstarTrainingDate.HasValue)
                        agent.FgRockstarTrainingDate = agentDto.FgRockstarTrainingDate;
                    if (agentDto.FgValueTrngDate.HasValue)
                        agent.FgValueTrngDate = agentDto.FgValueTrngDate;
                    if (agentDto.HSecPolicyTrngDate.HasValue)
                        agent.HSecPolicyTrngDate = agentDto.HSecPolicyTrngDate;
                    if (agentDto.ItSecPolicyTrngDate.HasValue)
                        agent.ItSecPolicyTrngDate = agentDto.ItSecPolicyTrngDate;
                    if (agentDto.NpsTrngCompletionDate.HasValue)
                        agent.NpsTrngCompletionDate = agentDto.NpsTrngCompletionDate;
                    if (agentDto.WhistleBlowerTrngDate.HasValue)
                        agent.WhistleBlowerTrngDate = agentDto.WhistleBlowerTrngDate;
                    if (agentDto.GovPolicyTrngDate.HasValue)
                        agent.GovPolicyTrngDate = agentDto.GovPolicyTrngDate;
                    if (agentDto.InductionTrngDate.HasValue)
                        agent.InductionTrngDate = agentDto.InductionTrngDate;
                    break;

                      default:
                          return BadRequest("Invalid section name");
                  }

                agent.ModifiedBy = username;
                agent.ModifiedDate = DateTime.UtcNow;

                // Compare snapshot and record scalar changes
                foreach (var kv in snapshot)
                {
                    var propName = kv.Key;
                    var oldVal = kv.Value;
                    object? newVal = null;
                    try
                    {
                        var pinfo = agent.GetType().GetProperty(propName);
                        if (pinfo != null)
                            newVal = pinfo.GetValue(agent);
                    }
                    catch { }

                    RecordChange(propName, oldVal, newVal);
                }

                await _context.SaveChangesAsync();

                hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hmsResponse.responseHeader.ErrorMessage = "SUCCESS";
                hmsResponse.responseBody.updatedAgentSectionName = sectionName;
                hmsResponse.responseBody.updatedAgentFields = updatedFields;
                return Ok(hmsResponse);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Record updated by another user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent {AgentId}",id);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}