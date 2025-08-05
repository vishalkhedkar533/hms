using HMS.Models;

namespace Models
{
    public class HmsDashboard
    {
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
        public List<ChannelMaster>? ChannelMasters { get; set; } = new List<ChannelMaster>();
    }
}
