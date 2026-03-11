namespace Models.DB
{

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Tasks.Models 
    {
        [Table("hms_dashboard", Schema = "hms")]
        public class HMSDashboard
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column("dashboard_id")]
            public int DashboardId { get; set; }

            [Column("orgid")]
            public long OrgId { get; set; }

            [Column("channel_id")]
            public long? ChannelId { get; set; }

            [Column("subchannel_id")]
            public long? SubchannelId { get; set; }

            [Column("totalentitiescount")]
            public double? TotalEntitiesCount { get; set; } = 0;

            [Column("totalentitiesthismonth")]
            public double? TotalEntitiesThisMonth { get; set; } = 0;

            [Column("entitiescreatedthismonth")]
            public double? EntitiesCreatedThisMonth { get; set; } = 0;

            [Column("entitiescreatedprevmonth")]
            public double? EntitiesCreatedPrevMonth { get; set; } = 0;

            [Column("entitiesterminatedthismonth")]
            public double? EntitiesTerminatedThisMonth { get; set; } = 0;

            [Column("entitiesterminatedprevmonth")]
            public double? EntitiesTerminatedPrevMonth { get; set; } = 0;

            [Column("entitiesnetthismonth")]
            public double? EntitiesNetThisMonth { get; set; } = 0;

            [Column("licenseexpiringin30months")]
            public double? LicenseExpiringIn30Months { get; set; } = 0;

            [Column("certificateexpiringin30months")]
            public double? CertificateExpiringIn30Months { get; set; } = 0;

            [Column("mbgcriterianotmet")]
            public double? MBGCriteriaNotMet { get; set; } = 0;

            [NotMapped]
            public List<ChannelDetails>? ChannelDetails { get; set; } = new List<ChannelDetails>();

            [NotMapped]
            public List<StatusDetails>? StatusDetails { get; set; } = new List<StatusDetails>();
        }
    }

    [Table("statusdetails", Schema = "hms")] // Quoted "HMSDashboard" handled via EF Table attribute
    public class StatusDetails
    {

        [Key]
        [Column("statusUserId")]
        public int UserId { get; set; }
        [Column("StatusId")]
        public long? StatusId { get; set; }
        [Column("StatusName")]
        public string? StatusName { get; set; } 
        [Column("PendingItem")]
        public long? PendingItem { get; set; } = 0;
        [Column("LastUpdated")]
        public string? LastUpdated { get; set; } 
        [Column("Priority")]
        public string? Priority { get; set; } 
    }
}
