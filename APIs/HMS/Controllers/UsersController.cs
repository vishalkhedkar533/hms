using AutoMapper;
using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using System.Security.Claims;
namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IAuthClaimService _authClaimService;
        private int orgId;
        private readonly ILogger<UsersController> _logger;

        public UsersController(HMSContext context, IConfiguration config, IMapper mapper, IAuthClaimService authClaimService, ILogger<UsersController> logger)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _authClaimService = authClaimService;
            _logger = logger;
        }
        //[Authorize]
        //[Authorize(Roles = "Admin")]
        [HttpPost("CreateUser")]
        [MenuAuthorize(AuthorisationConstants.ManagerUser)]
        public async Task<ActionResult<User>> CreateUser(UserCreateDto userDto)
        {
            HmsResponse response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            // 1. Check for existing users using the DTO properties
            if (await _context.Users.AsNoTracking().AnyAsync(u => u.Username == userDto.Username && u.OrgId == orgId))
            {
                response.responseHeader = new HmsSResponseHeader();
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Username already exists.";
                return Conflict(response);
            }

            if (await _context.Users.AsNoTracking().AnyAsync(u => u.EmailId == userDto.EmailId && u.OrgId == orgId))
            {
                response.responseHeader = new HmsSResponseHeader();
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Email ID already exists.";
                return Conflict(response);
            }
            // 2. Map DTO to Entity
            // Using AutoMapper, the mapping configuration handles defaults (IsActive=true, etc.)
            var user = _mapper.Map<User>(userDto);
            if (!string.IsNullOrEmpty(userDto.ReportingMgrName))
            {
                var reportingManager = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == userDto.ReportingMgrName && u.OrgId == orgId);
                if (reportingManager == null)
                {
                    response.responseHeader = new HmsSResponseHeader();
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Reporting Manager does not exist.";
                    return Conflict(response);
                }
                user.ReportingMgr = reportingManager.UserId;
            }
            user.CreatedBy = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier));
            user.OrgId = orgId;
            user.Username = user.Username.ToLower();
            // 3. Handle Password Hashing (Logic usually stays in Service or Controller)
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // 4. Persistence
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();

                var createdBy = user.CreatedBy.ToString();
                var now = DateTime.UtcNow;
                var createAuditEntries = new List<UserAuditTrail>
                {
                    new UserAuditTrail { OrgId = orgId, UserId = user.UserId, FieldName = "Username", OldValue = string.Empty, NewValue = user.Username, ChangedBy = createdBy, ChangedDate = now, CreatedBy = createdBy, CreatedDate = now },
                    new UserAuditTrail { OrgId = orgId, UserId = user.UserId, FieldName = "EmailId", OldValue = string.Empty, NewValue = user.EmailId, ChangedBy = createdBy, ChangedDate = now, CreatedBy = createdBy, CreatedDate = now },
                    new UserAuditTrail { OrgId = orgId, UserId = user.UserId, FieldName = "MobileNumber", OldValue = string.Empty, NewValue = user.MobileNumber ?? string.Empty, ChangedBy = createdBy, ChangedDate = now, CreatedBy = createdBy, CreatedDate = now },
                    new UserAuditTrail { OrgId = orgId, UserId = user.UserId, FieldName = "IsActive", OldValue = string.Empty, NewValue = user.IsActive.ToString(), ChangedBy = createdBy, ChangedDate = now, CreatedBy = createdBy, CreatedDate = now },
                    new UserAuditTrail { OrgId = orgId, UserId = user.UserId, FieldName = "IsLocked", OldValue = string.Empty, NewValue = user.IsLocked.ToString(), ChangedBy = createdBy, ChangedDate = now, CreatedBy = createdBy, CreatedDate = now },
                    new UserAuditTrail { OrgId = orgId, UserId = user.UserId, FieldName = "ReportingMgr", OldValue = string.Empty, NewValue = user.ReportingMgr?.ToString() ?? string.Empty, ChangedBy = createdBy, ChangedDate = now, CreatedBy = createdBy, CreatedDate = now }
                };

                await _context.UserAuditTrails.AddRangeAsync(createAuditEntries);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
            response.responseHeader = new HmsSResponseHeader();
            response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
            response.responseHeader.ErrorMessage = $"User {userDto.Username} created successfully";

            return Ok(response);
        }
        [HttpPost("UpdateUser")]
        [MenuAuthorize(AuthorisationConstants.ManagerUser)]
        public async Task<ActionResult> UpdateUser(UserOtherDetails userDto)
        {
            HmsResponse response = new HmsResponse();
            int orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            int currentUserId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            // 2. Check for conflicts (Username/Email) excluding the current user being updated
            if (await _context.Users.AsNoTracking().AnyAsync(u => u.Username == userDto.Username
            && u.OrgId == orgId && u.Username != userDto.Username))
            {
                response.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "Username already exists."
                };
                return Conflict(response);
            }

            if (await _context.Users.AsNoTracking().AnyAsync(u => u.EmailId == userDto.EmailId
            && u.OrgId == orgId && u.Username != userDto.Username))
            {
                response.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "Email ID already exists."
                };
                return Conflict(response);
            }

            int? reportingManagerId = null;

            if (!string.IsNullOrEmpty(userDto.ReportingMgrName))
            {
                reportingManagerId = (await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == userDto.ReportingMgrName && u.OrgId == orgId))?.UserId;
                if (reportingManagerId == null)
                {
                    response.responseHeader = new HmsSResponseHeader();
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Reporting Manager does not exist.";
                    return Conflict(response);
                }
            }

            // 1. Fetch the existing user, ensuring they belong to the current Org
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userDto.Username && u.OrgId == orgId);

            if (existingUser == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "User not found."
                };
                return Conflict(response);
            }

            var oldValues = new Dictionary<string, string?>
            {
                ["Username"] = existingUser.Username,
                ["EmailId"] = existingUser.EmailId,
                ["MobileNumber"] = existingUser.MobileNumber,
                ["IsActive"] = existingUser.IsActive.ToString(),
                ["IsLocked"] = existingUser.IsLocked.ToString(),
                ["ReportingMgr"] = existingUser.ReportingMgr?.ToString()
            };

            _mapper.Map(userDto, existingUser);

            // Update Reporting Manager
            existingUser.ReportingMgr = reportingManagerId;

            // 4. Update audit fields
            existingUser.ModifiedBy = currentUserId; // Or handle as int if your Model requires it
            existingUser.ModifiedDate = DateTime.UtcNow;

            var newValues = new Dictionary<string, string?>
            {
                ["Username"] = existingUser.Username,
                ["EmailId"] = existingUser.EmailId,
                ["MobileNumber"] = existingUser.MobileNumber,
                ["IsActive"] = existingUser.IsActive.ToString(),
                ["IsLocked"] = existingUser.IsLocked.ToString(),
                ["ReportingMgr"] = existingUser.ReportingMgr?.ToString()
            };

            var changedOn = DateTime.UtcNow;
            var auditEntries = oldValues
                .Where(kvp => (kvp.Value ?? string.Empty) != (newValues[kvp.Key] ?? string.Empty))
                .Select(kvp => new UserAuditTrail
                {
                    OrgId = orgId,
                    UserId = existingUser.UserId,
                    FieldName = kvp.Key,
                    OldValue = kvp.Value,
                    NewValue = newValues[kvp.Key],
                    ChangedBy = currentUserId.ToString(),
                    ChangedDate = changedOn,
                    CreatedBy = currentUserId.ToString(),
                    CreatedDate = changedOn,
                    ModifiedBy = currentUserId.ToString(),
                    ModifiedDate = changedOn
                })
                .ToList();

            if (auditEntries.Any())
            {
                await _context.UserAuditTrails.AddRangeAsync(auditEntries);
            }

            await _context.SaveChangesAsync();

            response.responseHeader = new HmsSResponseHeader
            {
                ErrorCode = CommonConstants.SUCCESS,
                ErrorMessage = "User updated successfully"
            };
            return Ok(response);
        }
        [HttpPost("UpdatePassword")]
        [MenuAuthorize(AuthorisationConstants.ResetPassword)]
        public async Task<ActionResult> UpdatePassword(UpdateUser request)
        {
            HmsResponse response = new HmsResponse();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
            if (user == null)
            {
                response.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "User not found."
                };
                return NotFound(response);
            }

            // Verify old password
            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);
            if (!isOldPasswordValid)
            {
                response.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "Old password is incorrect."
                };
                return BadRequest(response);
            }

            // Check if new password is same as old password
            bool isSamePassword = BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password);
            if (isSamePassword)
            {
                response.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "New password cannot be the same as the old password."
                };
                return BadRequest(response);
            }

            // Hash and update new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ModifiedDate = DateTime.UtcNow;
            user.PasswordChangedDate = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            response.responseHeader = new HmsSResponseHeader
            {
                ErrorCode = CommonConstants.SUCCESS,
                ErrorMessage = "Password updated successfully."
            };
            return Ok(response);
        }
        //[MenuAuthorize(AuthorisationConstants.ManagerUser)]
        //[HttpPost("ActivateDeactivateUser")]
        //public async Task<ActionResult<User>> DeactivateUser(UpdateUser request)
        //{
        //    HmsResponse response = new HmsResponse();
        //    var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
        //    if (currentUser == null)
        //    {
        //        response.responseHeader = new HmsSResponseHeader
        //        {
        //            ErrorCode = CommonConstants.FAILED,
        //            ErrorMessage = "User not found."
        //        };
        //        return NotFound(response);
        //    }

        //    // Hash the new password securely using BCrypt
        //    currentUser.IsActive = request.IsActive;

        //    // Update the user
        //    _context.Users.Update(currentUser);
        //    await _context.SaveChangesAsync();
        //    response.responseHeader = new HmsSResponseHeader
        //    {
        //        ErrorCode = CommonConstants.SUCCESS,
        //        ErrorMessage = $"User {(request.IsActive ? "activated" : "deactivated")} successfully."
        //    };

        //    return Ok(response);
        //}
        //[MenuAuthorize(AuthorisationConstants.ManagerUser)]
        //[HttpPost("LockUnlockUser")]
        //public async Task<ActionResult<User>> LockUnlockUser(UpdateUser request)
        //{
        //    var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
        //    if (currentUser == null)
        //    {
        //        return NotFound();
        //    }

        //    // Hash the new password securely using BCrypt
        //    currentUser.IsLocked = request.IsLocked;

        //    // Update the user
        //    _context.Users.Update(currentUser);
        //    await _context.SaveChangesAsync();

        //    return AcceptedAtAction(request.IsLocked ? "UserLocked" : "UserUnlocked", new { id = currentUser.UserId }, currentUser);
        //}
        [MenuAuthorize(AuthorisationConstants.ManagerUser)]
        [HttpPost("GetUserDetails")]
        public async Task<ActionResult<User>> GetUserDetails([FromBody] SearchUser SearchUser)
        {
            HmsResponse hmsResponse = new HmsResponse();
            if (string.IsNullOrWhiteSpace(SearchUser.Username) &&
                string.IsNullOrWhiteSpace(SearchUser.EmailId) &&
                string.IsNullOrWhiteSpace(SearchUser.MobileNumber))
            {
                hmsResponse.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "Please provide at least one of: username, emailId, or mobileNumber."
                };
                return BadRequest(hmsResponse);
            }

            var user = await _context.Users
                .AsNoTracking()
                .Where(u =>
                    (SearchUser.Username != null && u.Username == SearchUser.Username) ||
                    (SearchUser.EmailId != null && u.EmailId == SearchUser.EmailId) ||
                    (SearchUser.MobileNumber != null && u.MobileNumber == SearchUser.MobileNumber))
                // Manual Join to enforce the 'orgid = 2' condition
                .GroupJoin(_context.Users.Where(r => r.OrgId == 2),
                    u => u.ReportingMgr,
                    r => r.UserId,
                    (u, managers) => new { u, managers })
                .SelectMany(x => x.managers.DefaultIfEmpty(), (x, manager) => new { x.u, manager })
                .Select(combined => new UserOtherDetails
                {
                    UserId = combined.u.UserId,
                    Username = combined.u.Username,
                    EmailId = combined.u.EmailId,
                    MobileNumber = combined.u.MobileNumber,
                    IsActive = combined.u.IsActive,
                    IsLocked = combined.u.IsLocked,
                    LastLoggedInOn = combined.u.LastLoginDate,
                    PasswordChangedDate = combined.u.PasswordChangedDate,
                    FailedLoginAttempts = combined.u.failedloginattempts,
                    LockoutEndTime = combined.u.lockoutendtime,
                    ReportingMgrId = combined.u.ReportingMgr,
                    // The join result:
                    ReportingMgrName = combined.manager != null ? combined.manager.Username : null,
                    OrgName = combined.u.OrgName,
                    SubscriberId = combined.u.SubscriberId,
                    SubscriberName = combined.u.SubscriberName
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                hmsResponse.responseHeader = new HmsSResponseHeader
                {
                    ErrorCode = CommonConstants.FAILED,
                    ErrorMessage = "User not found."
                };
                return Conflict(hmsResponse);
            }

            hmsResponse.responseHeader = new HmsSResponseHeader
            {
                ErrorCode = CommonConstants.SUCCESS,
                ErrorMessage = "User details retrieved successfully."
            };
            user.ReportingMgrName = await _context.Users.AsNoTracking().Where(u => u.UserId == user.ReportingMgrId).Select(u => u.Username).FirstOrDefaultAsync();
            hmsResponse.responseBody.UserOtherDetails = new List<UserOtherDetails> { user };
            return Ok(hmsResponse);
        }
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
        private bool UserNameExists(string userName)
        {
            return _context.Users.Any(e => e.Username == userName);
        }
        // GET: api/Users
        //[Authorize]
        [HttpPost("GetUserIds")]
        [MenuAuthorize(AuthorisationConstants.ManagerUser)]
        public async Task<ActionResult<HmsResponse>> GetActiveUserIds([FromBody] SearchUser searchUser)
        {
            HmsResponse response = new HmsResponse();

            var orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            List<UserListDto> userList = await _context.Users
                .AsNoTracking()
                .Where(x => x.OrgId == orgId && 
                x.IsActive == (searchUser.IsActive == null ? x.IsActive : searchUser.IsActive))
                .Select(u => new UserListDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLocked,
                    FailedLoginAttempts = u.failedloginattempts
                })
                .ToListAsync();

            response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
            response.responseHeader.ErrorMessage = "User IDs retrieved successfully.";
            response.responseBody.UserList = userList;
            return Ok(response);
        }

        // GET: api/Users/5
        //[Authorize]
        //[HttpGet("{id}")]
        //public async Task<ActionResult<User>> GetUser(int id)
        //{
        //    var user = await _context.Users.FindAsync(id);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return user;
        //}

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[Authorize]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUser(int id, User user)
        //{
        //    if (id != user.UserId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(user).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("RegulatorUserBranch/Save")]
        [MenuAuthorize(AuthorisationConstants.ManagerUser)]
        public async Task<IActionResult> SaveRegulatorUserBranchMapping([FromBody] UserBranchMappingDto dto)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            var loggedInUserId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            if (dto is null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Request body is required.";
                return BadRequest(response);
            }

            var branchIds = dto.BranchIds?
                .Where(x => x > 0)
                .Distinct()
                .ToList() ?? new List<long>();

            if (branchIds.Count == 0)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "At least one valid BranchId is required.";
                return BadRequest(response);
            }

            try
            {
                var isUserValid = await _context.Users.AsNoTracking()
                    .AnyAsync(x => x.UserId == dto.UserId && x.OrgId == orgId);

                if (!isUserValid)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid UserId for the given Organisation.";
                    return Conflict(response);
                }

                var branches = await _context.BranchMaster.AsNoTracking()
                    .Where(x => x.OrgId == orgId && branchIds.Contains(x.BranchId))
                    .ToListAsync();

                if (branches.Count != branchIds.Count)
                {
                    var foundBranchIds = branches.Select(x => x.BranchId).ToHashSet();
                    var missingBranchIds = branchIds.Where(id => !foundBranchIds.Contains(id));
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = $"Invalid BranchId(s) for the given Organisation: {string.Join(",", missingBranchIds)}";
                    return Conflict(response);
                }

                var existingMappings = await _context.UserBranchMappings
                    .Where(x => x.OrgId == orgId && x.UserId == dto.UserId && branchIds.Contains(x.BranchId))
                    .ToListAsync();

                var existingBranchIds = existingMappings.Select(x => x.BranchId).ToHashSet();
                var newMappings = branchIds
                    .Where(branchId => !existingBranchIds.Contains(branchId))
                    .Select(branchId => new UserBranchMapping
                    {
                        OrgId = orgId,
                        UserId = dto.UserId,
                        BranchId = branchId,
                        CreatedBy = loggedInUserId,
                        CreatedDate = DateTime.UtcNow
                    })
                    .ToList();

                if (newMappings.Count > 0)
                    await _context.UserBranchMappings.AddRangeAsync(newMappings);

                foreach (var existing in existingMappings)
                {
                    existing.ModifiedBy = loggedInUserId;
                    existing.ModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "User regulator branch mappings saved successfully.";
                response.responseBody.userBranchMappings = existingMappings.Concat(newMappings).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving user regulator branch mappings.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("RegulatorUserBranch/FetchByUser/{UserId}")]
        [MenuAuthorize(AuthorisationConstants.ManagerUser)]
        public async Task<ActionResult<HmsResponse>> GetRegulatorBranchesByAgent([FromRoute] int UserId)
        {
            HmsResponse response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            try
            {
                var userMappedBranches = await (
                    from ubm in _context.UserBranchMappings.AsNoTracking()
                    join bm in _context.BranchMaster.AsNoTracking() on ubm.BranchId equals bm.BranchId
                    where ubm.OrgId == orgId && ubm.UserId == UserId
                    select new BranchListDto
                    {
                        BranchId = bm.BranchId,
                        BranchCode = bm.BranchCode,
                        BranchName = bm.BranchName,
                        IsActive = bm.IsActive,
                        IsReportedToRegulator = bm.IsReportedToRegulator
                    }).ToListAsync();

                if (userMappedBranches == null || userMappedBranches.Count == 0)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "No branches mapped to the given UserId.";
                    return NotFound(response);
                }
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.BranchList = userMappedBranches;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching regulator branches.");
                return StatusCode(500, ex.Message);
            }
        }
    }
}