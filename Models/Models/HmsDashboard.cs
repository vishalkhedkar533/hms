using HMS.Models;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class HmsDashboard
    {
        [Key]
        public Int64? UserId { get; set; } = 0;
        public Int64? TotalEntitiesCount { get; set; } = 10;
        public Int64? TotalEntitiesThisMonth { get; set; } = 20;
        public Int64? EntitiesCreatedThisMonth { get; set; } = 30;
        public Int64? EntitiesCreatedPrevMonth { get; set; } = 40;
        public Int64? EntitiesTerminatedThisMonth { get; set; } = 50;
        public Int64? EntitiesTerminatedPrevMonth { get; set; } = 60;
        public Int64? EntitiesNetThisMonth { get; set; } = 70;
        public Int64? LicenseExpiringIn30Months { get; set; } = 80;
        public Int64? CertificateExpiringIn30Months { get; set; } = 90;
        public Int64? MBGCriteriaNotMet { get; set; } = 100;
        //public List<ChannelMaster>? ChannelMasters { get; set; } = new List<ChannelMaster>();
        //public List<ChannelDetails>? channelDetails { get; set; } = new List<ChannelDetails>();
        //public List<StatusDetails>? statusDetails { get; set; } = new List<StatusDetails>();
    }
    public class ChannelDetails
    {
        [Key]
        public Int64? ChannelId { get; set; } 
        public string? ChannelName { get; set; } = "Digital Banking";
        public Int64? TotalEntities { get; set; } = 12;
        public Int64? Created { get; set; } = 10;
        public Int64? Terminated { get; set; } = 2;
    }

    public class StatusDetails
    {
        [Key]
        public Int64? StatusId { get; set; }
        public string? StatusName { get; set; } = "Code Movement";
        public Int64? PendingItem { get; set; } = 10;
        public string? LastUpdated { get; set; } = "2 hours ago";
        public string? Priority { get; set; } = "High";
    }
}
