namespace Models.DTO
{
    public class UpdateUser
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
    }
    public class FetchUserInfo 
    {
        public int? UserId { get; set; }
    }
}