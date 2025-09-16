using Models.DB;

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
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
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
