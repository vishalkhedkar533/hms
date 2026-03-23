namespace Models.DTO
{
    public class UpdateUser
    {
        public string Username { get; set; }
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public int? ReportingMgr { get; set; }
    }
    public class FetchUserDashboard
    {
        public int? UserId { get; set; }
    }
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}