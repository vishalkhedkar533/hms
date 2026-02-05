namespace SharedModels.BackEndCalculation
{
    public class Organisation
    {
        public int OrgId { get; set; }
        public int SubscriberId { get; set; }
        public string OrgName { get; set; } = string.Empty;
        public int? State { get; set; }
    }
}
