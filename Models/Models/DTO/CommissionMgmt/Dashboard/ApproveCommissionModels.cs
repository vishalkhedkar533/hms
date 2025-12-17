using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.CommissionMgmt.Dashboard
{
    public class ApproveCommissionResponseDto
    {
        public int OrgId { get; set; }
        public decimal TotalAmountApproved { get; set; }
        public int TotalRecords { get; set; }
        public int PendingApproval { get; set; }
        public List<ApproveCommissionLogDto>? Records { get; set; }
    }

    public class ApproveCommissionLogDto
    {
        public int ApprovalId { get; set; }
        public DateTime Date { get; set; }
        public string? Period { get; set; }
        public string? SubmittedBy { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public bool CanApprove { get; set; }
        public bool CanDownload { get; set; }
    }
}
