namespace Models.DB
{
    public static class ApiConstants
    {
        public static readonly string wrong_attempts_allowed = "wrong_attempts_allowed";
        public static readonly string encrypt_api_calls = "encrypt_api_calls";
        public static readonly string OrganisationId = "OrganisationId";
        public static readonly string OrganisationName = "OrganisationName";
        public static readonly string SubscriberId = "SubscriberId";
        public static readonly string SubscriberName = "SubscriberName";
    }
    public static class AuthorisationConstants
    {
        public const Int32 SearchAgent = 1030;
        public const Int32 ModifyAgent = 1002;
        public const Int32 FetchRoles = 1006;
        public const Int32 CreateRole=1007;
        public const Int32 DeleteRole = 1008;
        public const Int32 GetMenuAccessForRole = 1009;
        public const Int32 GetUserUnderRole = 1010;
        public const Int32 AddRemoveUserUnderRole = 1011;
        public const Int32 GrantRevokeMenuAccess = 1013;
        public const Int32 UpdateUIAccess = 1015;
        public const Int32 UIControlAccess = 1016;
        public const Int32 CreateUpdateDeleteChannel = 1018;
        public const Int32 SaveChannelDetails = 1019;
        public const Int32 CreateAgentUpdateSR = 1021;
        public const Int32 ApproveRejectAgentUpdateSR = 1022;
        public const Int32 FetchSRs = 1023;
        public const Int32 UpdateSRDecision = 1024;
        public const Int32 ManagerUser = 1026;
        public const Int32 ResetPassword = 1027;
        public const Int32 AuthenticationService = 1029;
    }
}
