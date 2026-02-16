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
        public const Int32 FetchRoles = 1006;
        public const Int32 CreateRole=1007;
        public const Int32 DeleteRole = 1008;
        public const Int32 GetMenuAccessForRole = 1009;
        public const Int32 GetUserUnderRole = 1010;
        public const Int32 AddUserUnderRole = 1011;
        public const Int32 RemoveUserFromRole = 1012;
        public const Int32 GrantMenuAccess = 1013;
        public const Int32 RevokeMenuAccess = 1014;
        public const Int32 UpdateUIAccess = 1015;
        public const Int32 UIControlAccess = 1016;
    }
}
