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
        public string? updatedAgentSectionName { get; set; } = null;
        public List<UpdatedAgentField>? updatedAgentFields { get; set; } = null;
        public List<JobExecutionHistoryDto>? jobExecutionHistory { get; set; } = null;
        public MetaDataResponse? metaDataResponse { get; set; } = null;
        public object? pagination { get; set; }
        public List<ProcessCommissionDTO>? processCommissionList { get; set; }
        public FileDownloadDto? fileDownload { get; set; } = null;
        public List<GeoHierarchyDto>? geoHierarchy { get; set; } = null;
        public UIMenuResponse? uiMenuResponse { get; set; } = null;
        public List<GeoHierarchyAgentDto>? geoAgentHierarchy { get; set; } = null;
        public List<BatchListDto>? batches { get; set; } = null;
        public List<Role>? roles { get; set; } = null;
        public List<MenuAccessDto>? MenuAccessList { get; set; } = null;
        public List<UserListDto>? UserList { get; set; } = null;
        public List<UserRoleMapping>? UserRoleMapping { get; set; } = null;
        public FileUploadResponse? fileUpload { get; set; } = null;
        public List<RoleMenuMapping>? RoleMenuMapping { get; set; } = null;
        public List<ChannelMaster>? channels { get; set; } = null;
        public List<SubChannelMaster>? subChannels { get; set; } = null;
        public List<DesignationMaster>? designations { get; set; } = null;
        public List<LocationMaster>? locations { get; set; } = null;
        public List<BranchMaster>? branches { get; set; } = null;
        public List<UiFieldsSetting>? uiFieldsSettings { get; set; } = null;
    }

    public class UpdatedAgentField
    {
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
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

    public class FileDownloadDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string FileBase64 { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class ChannelStatsResponse
    {
        public HmsSResponseHeader responseHeader { get; set; } = new HmsSResponseHeader();
        public ChannelStatsResponseBody responseBody { get; set; } = new ChannelStatsResponseBody();
    }

    public class ChannelStatsResponseBody
    {
        public List<ChannelMaster>? channels { get; set; } = null;
        public long totalEntities { get; set; }
        public long activeEntities { get; set; }
        public long terminatedEntities { get; set; }
    }
    public class FileUploadResponse
    {
        public int FileTaskId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }

}
