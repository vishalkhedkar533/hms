using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.CommissionMgmt.Dashboard
{
    public class ProcessCommissionLogDto
    {
        public int ProcessId { get; set; }
        public DateTime ProcessedDate { get; set; }
        public string? Period { get; set; }
        public int RecordsCount { get; set; }
        public string? Status { get; set; }
        public bool CanDownload { get; set; }
        public bool CanViewDetails { get; set; }
    }

    public class ProcessCommissionResponseDto
    {
        public int OrgId { get; set; }
        public string? PeriodType { get; set; }
        public List<ProcessCommissionLogDto>? ProcessedRecordsLog { get; set; }
    }
}
