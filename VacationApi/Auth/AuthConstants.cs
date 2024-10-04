namespace VacationApi.Auth
{
    public class AuthConstants
    {
        public static readonly string ApiKeySectionName = "Authentication:ApiKey";
        public static readonly string ApiKeyHeaderName = "X-Api-Key";
        public static readonly string AudiencePath = "JWT:ValidAudience";
        public static readonly string IssuerPath = "JWT:ValidIssuer";
        internal static readonly string SecretPath = "JWT:Secret";

        public static readonly string UID_KEY = "uid";
    }
}