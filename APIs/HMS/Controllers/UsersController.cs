using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
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
        public UsersController(HMSContext context, IConfiguration config, IMapper mapper, IAuthClaimService authClaimService)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _authClaimService = authClaimService;
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

            var reportingManager = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == userDto.ReportingMgrName && u.OrgId == orgId);
            if (reportingManager == null)
            {
                response.responseHeader = new HmsSResponseHeader();
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Reporting Manager does not exist.";
                return Conflict(response);
            }

            // 2. Map DTO to Entity
            // Using AutoMapper, the mapping configuration handles defaults (IsActive=true, etc.)
            var user = _mapper.Map<User>(userDto);
            user.CreatedBy = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier));
            user.OrgId = orgId;
            user.ReportingMgr = reportingManager.UserId;

            // 3. Handle Password Hashing (Logic usually stays in Service or Controller)
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // 4. Persistence
            _context.Users.Add(user);
            try
            {
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

            // 1. Fetch the existing user, ensuring they belong to the current Org
            var existingUser = await _context.Users.AsNoTracking()
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
            
            // 3. Map updated values from DTO to the existing Entity
            // AutoMapper will use the Condition(s) we set up previously to only update non-null fields
            _mapper.Map(userDto, existingUser);
            
            // Update Reporting Manager
            existingUser.ReportingMgr = reportingManagerId;

            // 4. Update audit fields
            existingUser.ModifiedBy = currentUserId; // Or handle as int if your Model requires it
            existingUser.ModifiedDate = DateTime.UtcNow;

            // 5. Save changes
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
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<User>>> GetUser()
        //{
        //    return await _context.Users.ToListAsync();
        //}

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
    }
}