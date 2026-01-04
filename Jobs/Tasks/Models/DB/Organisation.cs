namespace Tasks.Models.DB
{
    public class Organisation
    {
        public int OrgId { get; set; }
        public int SubscriberId { get; set; }
        public string OrgName { get; set; } = string.Empty;
    }
}
