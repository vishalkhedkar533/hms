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
using System.Security.Claims;

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
                    string.Empty,
                    refreshInterval)).Result.FirstOrDefault();

            var result = (_cacheService.GetRecordsAsync<KeyValueEntry>(
                masterTableConfigs?.SchemaName
                , masterTableConfigs?.TableName
                , Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0")
                , (masterTableConfigs?.FilterCriteria ?? string.Empty)
                , (masterTableConfigs?.columnalias ?? string.Empty)
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
                //var SalesSubChannels = AgentProfileMst.Where(x => x.EntryCategory == "SUB_CHANNEL");
                var State = AgentProfileMst.Where(x => x.EntryCategory == "STATE_NAME");
                var Occupations = AgentProfileMst.Where(x => x.EntryCategory == "OCCUPATION");
                var MaritalStatus = AgentProfileMst.Where(x => x.EntryCategory == "MARITAL_STATUS");
                var Gender = AgentProfileMst.Where(x => x.EntryCategory == "GENDER");
                var EducationQualification = AgentProfileMst.Where(x => x.EntryCategory == "EDUCATION_CODE");
                var Country = AgentProfileMst.Where(x => x.EntryCategory == "COUNTRY");
                //var SalesChannels = AgentProfileMst.Where(x => x.EntryCategory == "CHANNEL_NAME");
                var AgentTypeCategory = AgentProfileMst.Where(x => x.EntryCategory == "AGENT_TYPE_CAT");
                var Salutation = AgentProfileMst.Where(x => x.EntryCategory == "TITLE");
                var AgentType = AgentProfileMst.Where(x => x.EntryCategory == "AGNT_TYP");
                var CommissionClass = AgentProfileMst.Where(x => x.EntryCategory == "COMMISSION_CLASS");
                var CandidateType = AgentProfileMst.Where(x => x.EntryCategory == "CANDIDATE_TYP");
                var LicenceType = AgentProfileMst.Where(x => x.EntryCategory == "LICENSE_TYPE");
                var LicenceStatus = AgentProfileMst.Where(x => x.EntryCategory == "LICENSE_STATUS");
                var Vertical = AgentProfileMst.Where(x => x.EntryCategory == "VERTICAL");
                var TraningGroupType = AgentProfileMst.Where(x => x.EntryCategory == "TRAINING_GROUP");
                var Channel = GetMasterData("Channel").ToList();
                var SubChannel = GetMasterData("SubChannel").ToList();
                var Designation = GetMasterData("Designation").ToList();
                var Location = GetMasterData("Location").ToList();
                var Branch = GetMasterData("Branch").ToList();

                foreach (var bankAcc in agentDTO.bankAccounts)
                {
                    bankAcc.AccountTypeDesc = BankAccType
                        .Where(b => b.EntryIdentity == bankAcc.AccountType)
                        .Select(b => b.EntryDesc)
                        .FirstOrDefault() ?? string.Empty;
                }

                agentDTO.AgentClassDesc = AgentClass.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.AgentClass ??  -1000))?.EntryDesc ?? string.Empty;
                agentDTO.StateDesc = State.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.State ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.OccupationDesc = Occupations.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Occupation ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.MaritalStatusDesc = MaritalStatus.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.MaritalStatus ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.GenderDesc = Gender.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Gender ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.EducationDesc = EducationQualification.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Education ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.CountryDesc = Country.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Country ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.AgentTypeCodeDesc = AgentTypeCategory.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.AgentTypeCode ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.TitleDesc = Salutation.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Title ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.AgentTypeDesc = AgentType.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.AgentType ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.CommissionClassDesc = CommissionClass.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.CommissionClass ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.CandidateTypeDesc = CandidateType.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.CandidateType ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.LicenseTypeDesc = LicenceType.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.LicenseType ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.LicenseStatusDesc = LicenceStatus.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.LicenseStatus ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.VerticalDesc = Vertical.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.Vertical ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.TrainingGroupTypeDesc = TraningGroupType.SingleOrDefault(x=> x.EntryIdentity.Equals(agentDTO?.TrainingGroupType ?? -1000))?.EntryDesc?? string.Empty;
                agentDTO.ChannelDesc = Channel.SingleOrDefault(x => x.EntryIdentity.Equals(agentDTO?.Channel ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.SubChannelDesc = SubChannel.SingleOrDefault(x => x.EntryIdentity.Equals(agentDTO?.SubChannel ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.DesignationCodeDesc = Designation.SingleOrDefault(x => x.EntryIdentity.Equals(agentDTO?.DesignationCode ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.LocationCodeDesc = Location.SingleOrDefault(x => x.EntryIdentity.Equals(agentDTO?.LocationCode ?? -1000))?.EntryDesc ?? string.Empty;
                agentDTO.BranchDesc = Branch.SingleOrDefault(x => x.EntryIdentity.Equals(agentDTO?.Branch ?? -1000))?.EntryDesc ?? string.Empty;

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
        [MenuAuthorize(AuthorisationConstants.SearchAgent)]
        public async Task<IActionResult> AgentList()
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
        [MenuAuthorize(AuthorisationConstants.SearchAgent)]
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
        [MenuAuthorize(AuthorisationConstants.ModifyAgent)]
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
        [MenuAuthorize(AuthorisationConstants.ModifyAgent)]
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
        [MenuAuthorize(AuthorisationConstants.ModifyAgent)]
        public async Task<IActionResult> UpdateAgent([FromRoute] int id, [FromRoute] string sectionName,
         [FromBody] AgentDto agentDto)
        {
            ModelState.Clear();
            HmsResponse hmsResponse = new HmsResponse();
            var username = HttpContext?.User?.Identity?.Name ?? "System";
            var orgId = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            var saveInboxOnly = true;
            List<Inbox> inboxEntries = new List<Inbox>();
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
                var panNumberUpdated = false;

                void RecordChange(string field, object? oldVal, object? newVal)
                {
                    var oldS = oldVal == null ? string.Empty : (oldVal is DateTime odt ? odt.ToString("o") : oldVal.ToString());
                    var newS = newVal == null ? string.Empty : (newVal is DateTime ndt ? ndt.ToString("o") : newVal.ToString());
                    if (oldS != newS)
                    {
                        updatedFields.Add(new UpdatedAgentField { FieldName = field, OldValue = oldS ?? string.Empty, NewValue = newS ?? string.Empty });
                    }
                }

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
                    { "AgentTypeCat", agent.AgentTypeCat },
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
                    { "PANNumber", agent.PanNumber },
                    { "MainPartnerClientCode", agent.MainPartnerClientCode },
                    { "ApplicationDocketNo", agent.ApplicationDocketNo },
                    { "CandidateType", agent.CandidateType },
                    { "EmployeeCode", agent.EmployeeCode },
                    { "StartDate", agent.StartDate },
                    { "AppointmentDate", agent.AppointmentDate },
                    { "IncorporationDate", agent.IncorporationDate },
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
                    { "AdditionalComment", agent.AdditionalComment },
                    { "Channel", agent.Channel },
                    { "SubChannel", agent.SubChannel },
                    { "Ic36TrngCompletionDate", agent.Ic36TrngCompletionDate },
                    { "STrngCompletionDate", agent.STrngCompletionDate },
                    { "FgRockstarTrainingDate", agent.FgRockstarTrainingDate },
                    { "FgValueTrngDate", agent.FgValueTrngDate },
                    { "HSecPolicyTrngDate", agent.HSecPolicyTrngDate },
                    { "ItSecPolicyTrngDate", agent.ItSecPolicyTrngDate },
                    { "NpsTrngCompletionDate", agent.NpsTrngCompletionDate },
                    { "WhistleBlowerTrngDate", agent.WhistleBlowerTrngDate },
                    { "GovPolicyTrngDate", agent.GovPolicyTrngDate },
                    { "InductionTrngDate", agent.InductionTrngDate },
                    { "DesignationCode", agent.DesignationCode }

                };

                 switch ((sectionName ?? string.Empty).ToLowerInvariant())
                  {
                      case "individual_agent_action":
                        if (agentDto.Channel.HasValue)
                            agent.Channel = agentDto.Channel; 
                        if (agentDto.SubChannel.HasValue)
                            agent.SubChannel = agentDto.SubChannel; 
                        if (agentDto.DesignationCode.HasValue)
                            agent.DesignationCode = agentDto.DesignationCode;
                        if (agentDto.LocationCode.HasValue)
                            agent.LocationCode = agentDto.LocationCode;
                        
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

                        if (agentDto.AgentTypeCat.HasValue)
                            agent.AgentTypeCat = agentDto.AgentTypeCat;

                        if (agentDto.AgentClass.HasValue)
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
                          {
                              agent.PanNumber = agentDto.MaskedPanNumber;
                              panNumberUpdated = true;
                          }


                        if (agentDto.bankAccounts != null && agentDto.bankAccounts.Any())
                        {
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

                        if (agentDto.personalInfo != null && agentDto.personalInfo.Any())
                        {
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

                                    var oldAddressType = existingAddress.AddressType?.ToString() ?? string.Empty;
                                    var oldAddressLine1 = existingAddress.AddressLine1 ?? string.Empty;
                                    var oldAddressLine2 = existingAddress.AddressLine2 ?? string.Empty;
                                    var oldAddressLine3 = existingAddress.AddressLine3 ?? string.Empty;
                                    var oldCity = existingAddress.City ?? string.Empty;
                                    var oldState = existingAddress.State ?? string.Empty;
                                    var oldCountry = existingAddress.Country ?? string.Empty;
                                    var oldPin = existingAddress.PIN ?? string.Empty;
                                    var oldLandmark = existingAddress.Landmark ?? string.Empty;

                                    existingAddress.AddressType = addrDto.AddressType ?? existingAddress.AddressType;
                                    existingAddress.AddressLine1 = addrDto.AddressLine1 ?? existingAddress.AddressLine1;
                                    existingAddress.AddressLine2 = addrDto.AddressLine2 ?? existingAddress.AddressLine2;
                                    existingAddress.AddressLine3 = addrDto.AddressLine3 ?? existingAddress.AddressLine3;
                                    existingAddress.City = addrDto.City ?? existingAddress.City;
                                    existingAddress.State = addrDto.State ?? existingAddress.State;
                                    existingAddress.Country = addrDto.Country ?? existingAddress.Country;
                                    existingAddress.PIN = addrDto.PIN ?? existingAddress.PIN;
                                    existingAddress.Landmark = addrDto.Landmark ?? existingAddress.Landmark;

                                    _context.Address.Update(existingAddress);

                                    void AddIfChanged(string fieldName, string oldV, string newV)
                                    {
                                        if ((oldV ?? string.Empty) != (newV ?? string.Empty))
                                        {
                                            updatedFields.Add(new UpdatedAgentField { FieldName = fieldName, OldValue = oldV ?? string.Empty, NewValue = newV ?? string.Empty });
                                        }
                                    }

                                    AddIfChanged("AddressType",oldAddressType,existingAddress.AddressType?.ToString());
                                    AddIfChanged("AddressLine1", oldAddressLine1, existingAddress.AddressLine1);
                                    AddIfChanged("AddressLine2", oldAddressLine2, existingAddress.AddressLine2);
                                    AddIfChanged("AddressLine3", oldAddressLine3, existingAddress.AddressLine3);
                                    AddIfChanged("City", oldCity, existingAddress.City);
                                    AddIfChanged("State", oldState, existingAddress.State);
                                    AddIfChanged("Country", oldCountry, existingAddress.Country);
                                    AddIfChanged("PIN", oldPin, existingAddress.PIN);
                                    AddIfChanged("Landmark", oldLandmark, existingAddress.Landmark);

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

                                    updatedFields.Add(new UpdatedAgentField { FieldName = "AddressType", OldValue = string.Empty, NewValue = newAddress.AddressType?.ToString() ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "AddressLine1", OldValue = string.Empty, NewValue = newAddress.AddressLine1 ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "AddressLine2", OldValue = string.Empty, NewValue = newAddress.AddressLine2 ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "AddressLine3", OldValue = string.Empty, NewValue = newAddress.AddressLine2 ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "City", OldValue = string.Empty, NewValue = newAddress.City ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "State", OldValue = string.Empty, NewValue = newAddress.State ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "Country", OldValue = string.Empty, NewValue = newAddress.Country ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "PIN", OldValue = string.Empty, NewValue = newAddress.PIN ?? string.Empty });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "Landmark", OldValue = string.Empty, NewValue = newAddress.Landmark ?? string.Empty });

                                }
                            }
                            break;
                        }

                      case "license_details":
                        if (!string.IsNullOrWhiteSpace(agentDto.CnctPersonName))
                        agent.CnctPersonName = agentDto.CnctPersonName;

                        if (agentDto.AgentType.HasValue)
                            agent.AgentType = agentDto.AgentType;

                        if (agentDto.AgentClass.HasValue)
                            agent.AgentClass = agentDto.AgentClass;

                        if (agentDto.LicenseType.HasValue)
                            agent.LicenseType = agentDto.LicenseType;

                        if (agentDto.LicenseStatus.HasValue)
                            agent.LicenseStatus = agentDto.LicenseStatus;

                        if (agentDto.LicenseExpiryDate.HasValue)
                            agent.LicenseExpiryDate = agentDto.LicenseExpiryDate;

                        if (agentDto.LicenseIssueDate.HasValue)
                            agent.LicenseIssueDate = agentDto.LicenseIssueDate;

                        if(!string.IsNullOrWhiteSpace(agentDto.LicenseNo))
                            agent.LicenseNo = agentDto.LicenseNo;

                        agent.IsLicensed = agentDto.IsLicensed;

                        if (agentDto.CommissionClass.HasValue)
                            agent.CommissionClass = agentDto.CommissionClass;

                        break;

                      case "training_details":

                        if (agentDto.TrainingGroupType.HasValue)
                            agent.TrainingGroupType = agentDto.TrainingGroupType;

                        agent.RefresherTrainingCompleted = agentDto.RefresherTrainingCompleted;
                        break;

                      case "product_details":
                        agent.UlipFlag = agentDto.UlipFlag;
                        break;

                      case "others_details":

                        agent.IsMigrated = agentDto.IsMigrated;

                        if (!string.IsNullOrWhiteSpace(agentDto.MainPartnerClientCode))
                            agent.MainPartnerClientCode = agentDto.MainPartnerClientCode;

                        if (!string.IsNullOrWhiteSpace(agentDto.AgentMaincodevwEid))
                            agent.AgentMaincodeVweid = agentDto.AgentMaincodevwEid;

                        if (agentDto.RegistrationDate.HasValue)
                            agent.RegistrationDate = agentDto.RegistrationDate;

                        if (agentDto.Vertical.HasValue)
                            agent.Vertical = agentDto.Vertical;
                        break;

                      case "bank_details":
                        {
                            if (agentDto.BankAccType.HasValue)
                            {
                                var oldBankAccType = agent.BankAccType?.ToString() ?? string.Empty;
                                agent.BankAccType = agentDto.BankAccType;
                                RecordChange("BankAccType", oldBankAccType, agent.BankAccType?.ToString());
                            }

                            if (agentDto.bankAccounts != null && agentDto.bankAccounts.Any())
                            {
                                var b = agentDto.bankAccounts.First();

                                var existingBank = await _context.BankAccount
                                    .FirstOrDefaultAsync(x => x.RefKey == agent.AgentId && x.RefType == ReferenceType.Agent);

                                if (existingBank != null)
                                {
                                    // Capture old values
                                    var oldAccountHolder = existingBank.AccountHolderName ?? string.Empty;
                                    var oldAccountNumber = existingBank.AccountNumber ?? string.Empty;
                                    var oldIFSC = existingBank.IFSC ?? string.Empty;
                                    var oldMICR = existingBank.MICR ?? string.Empty;
                                    var oldBankName = existingBank.BankName ?? string.Empty;
                                    var oldBranchName = existingBank.BranchName ?? string.Empty;
                                    var oldAccountType = existingBank.AccountType.ToString();
                                    var oldFactoringHouse = existingBank.FactoringHouse ?? string.Empty;
                                    var oldPrefPayment = existingBank.PreferredPaymentMode.ToString();

                                    // Apply updates
                                    existingBank.AccountHolderName = b.AccountHolderName ?? existingBank.AccountHolderName;
                                    existingBank.AccountNumber = b.AccountNumber ?? existingBank.AccountNumber;
                                    existingBank.IFSC = b.IFSC ?? existingBank.IFSC;
                                    existingBank.MICR = b.MICR ?? existingBank.MICR;
                                    existingBank.BankName = b.BankName ?? existingBank.BankName;
                                    existingBank.BranchName = b.BranchName ?? existingBank.BranchName;
                                    existingBank.AccountType = b.AccountType != 0 ? b.AccountType : existingBank.AccountType;
                                    existingBank.FactoringHouse = b.FactoringHouse ?? existingBank.FactoringHouse;
                                    existingBank.PreferredPaymentMode = b.PreferredPaymentMode;
                                    existingBank.ActiveSince =existingBank.ActiveSince.HasValue? DateTime.SpecifyKind(existingBank.ActiveSince.Value, DateTimeKind.Utc): DateTime.UtcNow;

                                    _context.BankAccount.Update(existingBank);

                                    // Record field-level changes
                                    void AddIfChanged(string field, string oldV, string newV)
                                    {
                                        if ((oldV ?? string.Empty) != (newV ?? string.Empty))
                                        {
                                            updatedFields.Add(new UpdatedAgentField
                                            {
                                                FieldName = field,
                                                OldValue = oldV ?? string.Empty,
                                                NewValue = newV ?? string.Empty
                                            });
                                        }
                                    }

                                    AddIfChanged("AccountHolderName", oldAccountHolder, existingBank.AccountHolderName);
                                    AddIfChanged("AccountNumber", oldAccountNumber, existingBank.AccountNumber);
                                    AddIfChanged("IFSC", oldIFSC, existingBank.IFSC);
                                    AddIfChanged("MICR", oldMICR, existingBank.MICR);
                                    AddIfChanged("BankName", oldBankName, existingBank.BankName);
                                    AddIfChanged("BranchName", oldBranchName, existingBank.BranchName);
                                    AddIfChanged("AccountType", oldAccountType, existingBank.AccountType.ToString());
                                    AddIfChanged("FactoringHouse", oldFactoringHouse, existingBank.FactoringHouse);
                                    AddIfChanged("PreferredPaymentMode", oldPrefPayment, existingBank.PreferredPaymentMode.ToString());
                                }
                                else
                                {
                                    // Create new bank account
                                    var newBank = new BankAccount
                                    {
                                        RefKey = agent.AgentId,
                                        RefType = ReferenceType.Agent,
                                        AccountHolderName = b.AccountHolderName ?? $"{agent.FirstName} {agent.LastName}".Trim(),
                                        AccountNumber = b.AccountNumber ?? string.Empty,
                                        IFSC = b.IFSC ?? string.Empty,
                                        MICR = b.MICR,
                                        BankName = b.BankName,
                                        BranchName = b.BranchName,
                                        AccountType = b.AccountType != 0 ? b.AccountType : 1,
                                        FactoringHouse = b.FactoringHouse,
                                        PreferredPaymentMode = b.PreferredPaymentMode,
                                        ActiveSince = DateTime.UtcNow,
                                    };

                                    await _context.BankAccount.AddAsync(newBank);

                                    // Record created fields
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "AccountHolderName", OldValue = "", NewValue = newBank.AccountHolderName ?? "" });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "AccountNumber", OldValue = "", NewValue = newBank.AccountNumber ?? "" });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "IFSC", OldValue = "", NewValue = newBank.IFSC ?? "" });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "MICR", OldValue = "", NewValue = newBank.MICR ?? "" });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "BankName", OldValue = "", NewValue = newBank.BankName ?? "" });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "BranchName", OldValue = "", NewValue = newBank.BranchName ?? "" });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "AccountType", OldValue = "", NewValue = newBank.AccountType.ToString() });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "FactoringHouse", OldValue = "", NewValue = newBank.FactoringHouse ?? "" });
                                    updatedFields.Add(new UpdatedAgentField { FieldName = "PreferredPaymentMode", OldValue = "", NewValue = newBank.PreferredPaymentMode.ToString() });
                                }
                            }

                            break;
                        }

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

                    //Below Case Not implemented as per discussion 
                    #region Not Implemented
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
                    #endregion
                    default:
                          return BadRequest("Invalid section name");
                  }

                agent.ModifiedBy = username;
                agent.ModifiedDate = DateTime.UtcNow;

                foreach (var kv in snapshot)
                {
                    var propName = kv.Key;
                    if (!panNumberUpdated && string.Equals(propName, "PANNumber", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

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
                if (updatedFields.Any())
                {
                    if (!saveInboxOnly)
                    {
                        var auditEntries = updatedFields.Select(f => new AgentAuditTrail
                        {
                            AgentId = id,
                            FieldName = f.FieldName,
                            OldValue = f.OldValue,
                            NewValue = f.NewValue,
                            ChangedBy = username,
                            ChangedDate = DateTime.UtcNow,
                            CreatedBy = username,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedBy = username,
                            ModifiedDate = DateTime.UtcNow
                        }).ToList();

                        await _context.AgentAuditTrail.AddRangeAsync(auditEntries);
                    }

                    //inbox entries
                    var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var createdBy = int.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : 0;
                    var orgIdValue = Convert.ToInt32(orgId);
                    var fieldNames = updatedFields
                        .Select(f => f.FieldName)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Select(n => n.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (fieldNames.Any())
                    {
                        string Normalize(string value)
                        {
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                return string.Empty;
                            }

                            var cleaned = new string(value.Where(char.IsLetterOrDigit).ToArray());
                            return cleaned.ToLowerInvariant();
                        }

                        Dictionary<int, string> ToMap(IEnumerable<KeyValueEntry> entries) => entries
                            .GroupBy(e => e.EntryIdentity)
                            .ToDictionary(g => g.Key, g => g.First().EntryDesc ?? string.Empty);

                        var agentProfileMst = GetMasterData("AgentProfileMst");
                        Dictionary<int, string> GetCategoryMap(string category) => ToMap(agentProfileMst
                            .Where(x => string.Equals(x.EntryCategory, category, StringComparison.OrdinalIgnoreCase)));

                        var fieldValueLookup = new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["Title"] = GetCategoryMap("TITLE"),
                            ["Gender"] = GetCategoryMap("GENDER"),
                            ["MaritalStatus"] = GetCategoryMap("MARITAL_STATUS"),
                            ["Education"] = GetCategoryMap("EDUCATION_CODE"),
                            ["Occupation"] = GetCategoryMap("OCCUPATION"),
                            ["AgentClass"] = GetCategoryMap("AGENT_CLASS"),
                            ["AgentType"] = GetCategoryMap("AGNT_TYP"),
                            ["AgentTypeCat"] = GetCategoryMap("AGENT_TYPE_CAT"),
                            ["AgentTypeCode"] = GetCategoryMap("AGENT_TYPE_CAT"),
                            ["CandidateType"] = GetCategoryMap("CANDIDATE_TYP"),
                            ["CommissionClass"] = GetCategoryMap("COMMISSION_CLASS"),
                            ["BankAccType"] = GetCategoryMap("BANK_ACC_TYP"),
                            ["LicenseType"] = GetCategoryMap("LICENSE_TYPE"),
                            ["LicenseStatus"] = GetCategoryMap("LICENSE_STATUS"),
                            ["Vertical"] = GetCategoryMap("VERTICAL"),
                            ["TrainingGroupType"] = GetCategoryMap("TRAINING_GROUP"),
                            ["State"] = GetCategoryMap("STATE_NAME"),
                            ["Country"] = GetCategoryMap("COUNTRY"),
                            ["Channel"] = ToMap(GetMasterData("Channel")),
                            ["SubChannel"] = ToMap(GetMasterData("SubChannel")),
                            ["DesignationCode"] = ToMap(GetMasterData("Designation")),
                            ["LocationCode"] = ToMap(GetMasterData("Location")),
                            ["Branch"] = ToMap(GetMasterData("Branch"))
                        };

                        string ResolveDisplayValue(string fieldName, string value)
                        {
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                return value;
                            }

                            if (!int.TryParse(value, out var id))
                            {
                                return value;
                            }

                            if (string.Equals(fieldName, "AccountType", StringComparison.OrdinalIgnoreCase))
                            {
                                return Enum.GetName(typeof(BankAccType), id) ?? value;
                            }

                            if (string.Equals(fieldName, "PreferredPaymentMode", StringComparison.OrdinalIgnoreCase))
                            {
                                return Enum.GetName(typeof(PreferredPaymentMode), id) ?? value;
                            }

                            if (string.Equals(fieldName, "AddressType", StringComparison.OrdinalIgnoreCase))
                            {
                                return Enum.GetName(typeof(AddressType), id) ?? value;
                            }

                            if (!fieldValueLookup.TryGetValue(fieldName, out var map))
                            {
                                return value;
                            }

                            return map.TryGetValue(id, out var desc) && !string.IsNullOrWhiteSpace(desc) ? desc : value;
                        }

                        var uiFields = await _context.uiField
                            .AsNoTracking()
                            .Select(f => new { f.CntrlId, f.CntrlName })
                            .ToListAsync();

                        var normalizedUiFields = uiFields
                            .Select(f => new { f.CntrlId, f.CntrlName, Normalized = Normalize(f.CntrlName) })
                            .ToList();

                        var controlIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                        foreach (var fieldName in fieldNames)
                        {
                            var normalizedField = Normalize(fieldName);
                            if (string.IsNullOrWhiteSpace(normalizedField))
                            {
                                continue;
                            }

                            var match = normalizedUiFields.FirstOrDefault(f => f.Normalized == normalizedField)
                                        ?? normalizedUiFields.FirstOrDefault(f => f.Normalized.Contains(normalizedField)
                                                                              || normalizedField.Contains(f.Normalized));

                            if (match != null && match.CntrlId != 0)
                            {
                                controlIdMap[fieldName] = match.CntrlId;
                            }
                        }

                        var controlIds = controlIdMap.Values.Distinct().ToList();
                        var allocationLookup = await _context.uiFieldsSettings
                            .AsNoTracking()
                            .Where(s => s.OrgId == orgIdValue && s.CntrlId.HasValue && controlIds.Contains(s.CntrlId.Value))
                            .Select(s => new { s.CntrlId, s.ApproverOneId, s.RoleId })
                            .ToListAsync();

                        var allocatedRoleMap = allocationLookup
                            .GroupBy(x => x.CntrlId!.Value)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.ApproverOneId ?? x.RoleId).FirstOrDefault());

                        inboxEntries = updatedFields
                            .Where(f => controlIdMap.TryGetValue(f.FieldName, out var cntrlId) && cntrlId != 0)
                            .Select(f =>
                            {
                                var cntrlId = controlIdMap[f.FieldName];
                                allocatedRoleMap.TryGetValue(cntrlId, out var allocatedRole);
                                return new Inbox
                                {
                                    OrgId = orgIdValue,
                                    CreatedBy = createdBy,
                                    CreatedDate = DateTime.UtcNow,
                                    SrStatus = SrStatus.Created,
                                    RequestDets = $"{f.FieldName} updated",
                                    RequestorNote = $"Old Value: {ResolveDisplayValue(f.FieldName, f.OldValue)} | New Value: {ResolveDisplayValue(f.FieldName, f.NewValue)}",
                                    ControlId = cntrlId,
                                    AllocatedToRole = allocatedRole
                                };
                            })
                            .ToList();

                        if (inboxEntries.Any())
                        {
                            await _context.Inbox.AddRangeAsync(inboxEntries);
                        }
                    }
                }

                if (saveInboxOnly)
                {
                    foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.Entity is not Inbox))
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entry.State = EntityState.Detached;
                                break;
                            case EntityState.Modified:
                            case EntityState.Deleted:
                                entry.State = EntityState.Unchanged;
                                break;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hmsResponse.responseHeader.ErrorMessage = "SUCCESS";
                hmsResponse.responseBody.updatedAgentSectionName = sectionName;
                hmsResponse.responseBody.updatedAgentFields = updatedFields;
                hmsResponse.responseBody.InboxData = inboxEntries;
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

        [HttpPost("GeoHierarchy")]
        [MenuAuthorize(AuthorisationConstants.SearchAgent)]
        public async Task<IActionResult> GetGeoHierarchy([FromBody] GeoSearchRequest request)
        {
            HmsResponse hMSResponse = new HmsResponse();
            long orgId = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            try
            {
                if (request?.ChannelCode == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_GEOHEIRARCHY_NOTFOUND;
                    hMSResponse.responseHeader.ErrorMessage = "Geo Hierarchy not found for this selection.";
                    return BadRequest(hMSResponse);
                }


                var stringResponse = await _db.ExecuteQueryAsync<string>(
                    "Agent",
                    "get_geo_hierarchy",
                    new
                    {
                        p_channel_id = request.ChannelCode,
                        p_subchannel_id = request.SubChannelCode, // Pass null if sub-channel isn't provided
                        p_orgid = orgId,
                        p_branch_id = request.BranchCode // Pass null if branch isn't provided
                    });

                if (!string.IsNullOrEmpty(stringResponse.FirstOrDefault()))
                {
                    var geoData = JsonConvert.DeserializeObject<List<GeoHierarchyDto>>(
                        stringResponse.FirstOrDefault(),
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });

                    hMSResponse.responseHeader.ErrorCode = 1101;
                    hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                    hMSResponse.responseBody.geoHierarchy = geoData;
                    return Ok(hMSResponse);
                }
                else
                {
                    hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_GEOHEIRARCHY_NOTFOUND;
                    hMSResponse.responseHeader.ErrorMessage = "Geo Hierarchy not found for this selection.";
                    return NotFound(hMSResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred In GeoHierarchy");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("ReporteesByChannelLocation")]
        [MenuAuthorize(AuthorisationConstants.SearchAgent)]
        public async Task<IActionResult> GetReporteesByChannelLocation([FromBody] ReporteesByLocationRequest request)
        {
            var channelCode = request.ChannelCode.Trim();
            var locationCode = request.LocationCode.Trim();
            var orgId = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            try
            {
                var channelId = await _context.ChannelMaster
                    .AsNoTracking()
                    .Where(c => c.ChannelCode == channelCode && c.OrgId == orgId)
                    .Select(c => (long?)c.ChannelId)
                    .FirstOrDefaultAsync();

                if (!channelId.HasValue)
                    return NotFound("Channel code not found.");

                var locationId = await _context.LocationMasters
                    .AsNoTracking()
                    .Where(l => l.LocationCode == locationCode && l.OrgId == (int)orgId)
                    .Select(l => (long?)l.LocationMasterId)
                    .FirstOrDefaultAsync();

                if (!locationId.HasValue)
                    return NotFound("Location code not found.");

                HmsResponse hMSResponse = new HmsResponse();


                var reportees = await _db.ExecuteQueryAsync<GeoHierarchyAgentDto>(
                    "Agent",
                    "get_reportees_by_channel_location",
                    new
                    {
                        channel = channelId.Value,
                        location_id = locationId.Value,
                        p_orgid = orgId
                    });

                if (reportees != null && reportees.Any())
                {
                    hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                    hMSResponse.responseBody.geoAgentHierarchy = reportees.ToList();
                    return Ok(hMSResponse);
                }
                else
                {
                    hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                    hMSResponse.responseHeader.ErrorMessage = "No reportees found for this location.";
                    return NotFound(hMSResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred In GetReporteesByChannelLocation");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("GeoChildren")]
        [MenuAuthorize(AuthorisationConstants.SearchAgent)]
        public async Task<IActionResult> GetGeoChildren([FromBody] GeoChildrenRequest request)
        {
            HmsResponse hMSResponse = new HmsResponse();
            long orgId = Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            try
            {
                var stringResponse = await _db.ExecuteQueryAsync<string>(
                    "Agent",
                    "get_geo_children",
                    new
                    {
                        p_branch_id = request.ParentBranchId,
                        p_orgid = (int)orgId
                    });

                if (!string.IsNullOrEmpty(stringResponse.FirstOrDefault()))
                {
                    var childrenData = JsonConvert.DeserializeObject<List<GeoHierarchyDto>>(
                        stringResponse.FirstOrDefault(),
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                    );

                    hMSResponse.responseHeader.ErrorCode = 1101;
                    hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                    hMSResponse.responseBody.geoHierarchy = childrenData;
                    return Ok(hMSResponse);
                }

                return NotFound("No sub-offices found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred In GeoChildren");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}