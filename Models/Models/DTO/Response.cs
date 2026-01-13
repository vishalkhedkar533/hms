using Models.DB;
using Models.DTO.CommissionMgmt;
using Models.DTO.CommissionMgmt.Dashboard;
using SharedModels;

namespace Models.DTO
{
    public class HmsResponse
    {
        public HmsSResponseHeader responseHeader { get; set; } = new HmsSResponseHeader();
        public HmsResponseBody responseBody { get; set; } = new HmsResponseBody();
    }

    public class HmsSResponseHeader
    {
        public Int32 ErrorCode { get; set; } = 0;
        public string ErrorMessage { get; set; } = string.Empty;
        
    }
    public class HmsResponseBody 
    {
        public LoginResponse? loginResponse { get; set; } = null;
        public HMSDashboard? hmsDashboard { get; set; } = null;
        //public List<AgentDtoResponse>? agents { get; set; } = null;
        public List<AgentDto>? agents { get; set; } = null;
        public List<KeyValueEntry>? master { get; set; } = null;
        public List<CommissionMgmtDashboardDto>? commissionMgmtDashboards { get; set; } = null;
        public ProcessCommissionResponseDto? processCommission { get; set; } = null;
        public HoldCommissionResponseDto? holdCommission { get; set; } = null;
        public AdjustCommissionResponseDto? adjustCommission { get; set; }
        public ApproveCommissionResponseDto? approveCommission { get; set; }
        public List<CommissionConfigDTO>? commissionConfig { get; set; } = null;
        public List<JobExecutionHistoryDto>? jobExecutionHistory { get; set; } = null;
        public MetaDataResponse? metaDataResponse { get; set; } = null;
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public bool? Encrypt_Api_Calls { get; set; } = false;

    }

    public class OtpResponse
    {
        public HmsSResponseHeader responseHeader { get; set; } = new HmsSResponseHeader();
        public OtpResponseBody responseBody { get; set; } = new OtpResponseBody();
    }
    public class OtpResponseBody
    {
        public string username { get; set; }
        public string otp { get; set; }
        
    }

}
