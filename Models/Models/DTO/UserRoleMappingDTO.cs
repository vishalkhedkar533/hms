namespace Models.DTO
{
    public class UserRoleMappingDTO
    {
        public string UserName { get; set; } = null!;
        public int RoleId { get; set; }
    }

    public class RoleMenuDTO 
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }
    }
}
