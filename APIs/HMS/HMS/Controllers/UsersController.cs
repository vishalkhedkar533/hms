using HMS.Data;
using HMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;


namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;

        public UsersController(HMSContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //[Authorize]
        //[Authorize(Roles = "Admin")]
        [HttpPost("CreateUser")]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return Conflict("Username already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.EmailId == user.EmailId))
            {
                return Conflict("Email already exists.");
            }

            // Hash the password using BCrypt
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Set default values
            user.IsActive = true;
            user.IsLocked = false;
            user.CreatedDate = DateTime.UtcNow;
            user.ModifiedDate = null;
            user.RowVersion = 1;
            user.PasswordChangedDate = DateTime.UtcNow;
            user.failedloginattempts = 0;
            user.lockoutendtime = null;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully", user.UserId });
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("UpdatePassword")]
        public async Task<ActionResult> UpdatePassword(UpdateUser request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Verify old password
            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);
            if (!isOldPasswordValid)
            {
                return BadRequest("Old password is incorrect.");
            }

            // Check if new password is same as old password
            bool isSamePassword = BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password);
            if (isSamePassword)
            {
                return BadRequest("New password cannot be the same as the old password.");
            }

            // Hash and update new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ModifiedDate = DateTime.UtcNow;
            user.PasswordChangedDate = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Password updated successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ActivateDeactivateUser")]
        public async Task<ActionResult<User>> DeactivateUser(UpdateUser request)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Hash the new password securely using BCrypt
            currentUser.IsActive = request.IsActive;

            // Update the user
            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();

            return AcceptedAtAction(request.IsActive ? "UserActivated" : "UserDeActivated", new { id = currentUser.UserId }, currentUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("LockUnlockUser")]
        public async Task<ActionResult<User>> LockUnlockUser(UpdateUser request)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Hash the new password securely using BCrypt
            currentUser.IsLocked = request.IsLocked;

            // Update the user
            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();

            return AcceptedAtAction(request.IsLocked ? "UserLocked" : "UserUnlocked", new { id = currentUser.UserId }, currentUser);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("GetUserDetails")]
        public async Task<ActionResult<User>> GetUserDetails([FromBody] User SearchUser)
        {
            if (string.IsNullOrWhiteSpace(SearchUser.Username) &&
                string.IsNullOrWhiteSpace(SearchUser.EmailId) &&
                string.IsNullOrWhiteSpace(SearchUser.MobileNumber))
            {
                return BadRequest("Please provide at least one of: username, emailId, or mobileNumber.");
            }

            var user = await _context.Users
                .Where(u =>
                    (SearchUser.Username != null && u.Username == SearchUser.Username) ||
                    (SearchUser.EmailId != null && u.EmailId == SearchUser.EmailId) ||
                    (SearchUser.MobileNumber != null && u.MobileNumber == SearchUser.MobileNumber))
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Optional: Avoid returning sensitive data (like password)
            var result = new
            {
                user.UserId,
                user.Username,
                user.EmailId,
                user.MobileNumber,
                user.IsActive,
                user.IsLocked,
                user.LastLoginDate,
                user.CreatedDate,
                user.ModifiedDate,
                user.failedloginattempts,
                user.lockoutendtime
            };

            return Ok(result);
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
