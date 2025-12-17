using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.CommissionMgmt.Dashboard
{
    public class AdjustCommissionResponseDto
    {
        public int OrgId { get; set; }
        public int Approved { get; set; }
        public int PendingReview { get; set; }
        public int Rejected { get; set; }
        public int TotalRecords { get; set; }
        public List<AdjustCommissionLogDto>? Records { get; set; }
    }

    public class AdjustCommissionLogDto
    {
        public int AdjustmentId { get; set; }
        public DateTime Date { get; set; }
        public string? Period { get; set; }
        public string? AdjustmentType { get; set; }
        public string? UploadedBy { get; set; }
        public int RecordsCount { get; set; }
        public string? Status { get; set; }
    }
}
