namespace Models.DTO
{
    public class UiFieldsSettingDto
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public int? CntrlId { get; set; }
        // Settings with default values matching your DB logic
        public bool Render { get; set; } = true;
        public bool AllowEdit { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        // Permissions and Approvers
        public int? RoleId { get; set; }
        public int? ApproverOneId { get; set; }
        public int? ApproverTwoId { get; set; }
        public int? ApproverThreeId { get; set; }
        public bool UseDefaultApprover { get; set; } = true;
    }
}
