namespace Models.HMSConsts
{
    public static class LoginConstants //10x series
    {
        public const int INVALID_CREDENTIALS = 1001;
        public const int ACCOUNT_LOCKED = 1002;
        public const int NO_ACTIVE_PRIMARY_ROLE = 1003;
    }
    public static class CommonConstants //11x series
    {
        public const int SUCCESS = 1101;
        public const int FAILED = 4001;
    }
    public static class AgentConstants //12x series
    {
        public const int AGENT_NOTFOUND = 1201;
        public const int AGENT_GEOHEIRARCHY_NOTFOUND = 1201;

    }
    public static class MastersConstants //13x series
    {
        public const int MASTER_NOTFOUND = 1301;
    }
    public static class AddressTypeConstants //12x series
    {
        public const int PERMANENT = 1301;
        public const int CORRESPONDENCE_1 = 1302;
        public const int CORRESPONDENCE_2 = 1303;
    }
    public static class CommissionConstants  //14x series
    {
        public const int COMMISSION_NOTFOUND = 1401;
    }
    public static class JobConstants  //15x series
    {
        public const int JOB_NOTFOUND = 1501;
    }
}