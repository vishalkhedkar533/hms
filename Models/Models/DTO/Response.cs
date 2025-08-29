using Models.DB;

namespace Models.DTO
{
    public class HMSResponse
    {
        public HMSResponseHeader responseHeader { get; set; } = new HMSResponseHeader();
        public HMSResponseBody responseBody { get; set; } = new HMSResponseBody();
    }

    public class HMSResponseHeader
    {
        public Int32 ErrorCode { get; set; } = 0;
        public string ErrorMessage { get; set; } = string.Empty;
        
    }
    public class HMSResponseBody 
    {
        public LoginResponse? loginResponse { get; set; } = null;
        public HMSDashboard? hmsDashboard { get; set; } = null;
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
