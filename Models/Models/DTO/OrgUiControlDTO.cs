namespace Models.DTO
{
    public class OrgUiControlDTO
    {
        public long UiControlMenuId { get; set; }
        public long RoleId { get; set; }
        public bool AllowRead { get; set; } = false;
        public bool AllowEdit { get; set; } = false;
        public bool? RenderControl { get; set; } = false;
    }
}