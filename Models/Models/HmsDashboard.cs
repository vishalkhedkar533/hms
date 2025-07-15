using HMS.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class HmsDashboard
    {
        public Int64? TotalEntitiesCount { get; set; }
        public Int64? TotalEntitiesThisMonth { get; set; }
        public Int64? EntitiesCreatedThisMonth { get; set; }
        public Int64? EntitiesCreatedPrevMonth { get; set; }
        public Int64? EntitiesTerminatedThisMonth { get; set; }
        public Int64? EntitiesTerminatedPrevMonth { get; set; }
        public Int64? EntitiesNetThisMonth { get; set; }
        public Int64? LicenseExpiringIn30Months { get; set; }
        public Int64? CertificateExpiringIn30Months { get; set; }
        public Int64? MBGCriteriaNotMet { get; set; }
        public List<ChannelMaster>? ChannelMasters { get; set; }
    }
}
