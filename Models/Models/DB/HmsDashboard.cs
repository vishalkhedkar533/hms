using Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
  
    [Table("hmsdashboard", Schema = "hms")] // Quoted "HMSDashboard" handled via EF Table attribute
    public class HMSDashboard
    {
        [Key]
        [Column("UserId")]
        public int UserId { get; set; }
        [Column("TotalEntitiesCount")]
         
        public double? TotalEntitiesCount { get; set; } = 0;
        [Column("TotalEntitiesThisMonth")]
        public double? TotalEntitiesThisMonth { get; set; } = 0;
        [Column("EntitiesCreatedThisMonth")]
        public double? EntitiesCreatedThisMonth { get; set; } = 0;
        [Column("EntitiesCreatedPrevMonth")]
        public double? EntitiesCreatedPrevMonth { get; set; } = 0;
        [Column("EntitiesTerminatedThisMonth")]
        public double? EntitiesTerminatedThisMonth { get; set; } = 0;
        [Column("EntitiesTerminatedPrevMonth")]
        public double? EntitiesTerminatedPrevMonth { get; set; } = 0;
        [Column("EntitiesNetThisMonth")]
        public double? EntitiesNetThisMonth { get; set; } = 0;
        [Column("LicenseExpiringIn30Months")]
        public double? LicenseExpiringIn30Months { get; set; } = 0;
        [Column("CertificateExpiringIn30Months")]
        public double? CertificateExpiringIn30Months { get; set; } = 0;
        [Column("MBGCriteriaNotMet")]
        public double? MBGCriteriaNotMet { get; set; } = 0;

        public List<ChannelDetails>? channelDetails { get; set; } = new List<ChannelDetails>();
        public List<StatusDetails>? statusDetails { get; set; } = new List<StatusDetails>();
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
