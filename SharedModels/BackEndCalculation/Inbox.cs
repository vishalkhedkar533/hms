namespace SharedModels.BackEndCalculation
{
    public  class InboxEntry
    {
        public int SrNo { get; set; }
        public int OrgId { get; set; }
        public int? ComponentId { get; set; }
        public int CreatedBy { get; set; }
        public string? ApprovalEndpoint { get; set; }
        public string? ApprovalPayload { get; set; }
    }

    public  class InboxFieldConfig
    {
        public int? ComponentId { get; set; }
        public string? Label { get; set; }
        public string? ElementType { get; set; }
        public int? ControlId { get; set; }
        public string? ControlName { get; set; }
        public int? RoleId { get; set; }
        public bool? UseDefaultApprover { get; set; }
        public int? ApproverOneId { get; set; }
        public int? ApproverTwoId { get; set; }
        public int? ApproverThreeId { get; set; }
    }
}
