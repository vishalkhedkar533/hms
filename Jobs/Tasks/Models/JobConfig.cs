namespace Models
{
    public class JobConfig
    {
        public int Job_Config_Id { get; set; }
        public string Job_Name { get; set; } = string.Empty;
        public string Job_Type { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string? Trigger_Type { get; set; }
        public string? Cron_Expression { get; set; }
        public int? Interval_Seconds { get; set; }
        public DateTimeOffset? Start_At { get; set; }
        public DateTimeOffset? End_At { get; set; }
        public string? Parameters { get; set; }
        public DateTimeOffset Created_At { get; set; }
        public DateTimeOffset? Updated_At { get; set; }
        public string? TargetType { get; set; }
        public string? TargetMethod { get; set; }
        public string? Args { get; set; }
        public int? orgid { get; set; }

        // convenience
        public int Id => Job_Config_Id;
        public string Name => Job_Name;
        public string Type => Job_Type;
    }
}