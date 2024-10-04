using VacationApi.Auth;

namespace VacationApi.Utils
{
    public static class HttpContextExtensionMethods
    {
        public static String ConnectedUserId(this HttpContext httpContext)
        {
            return httpContext.User.Claims.First(x => x.Type == AuthConstants.UID_KEY).Value;
        }
        public static String ConnectedUserId(this IHttpContextAccessor httpContext)
        {
            return httpContext.HttpContext.User.Claims.First(x => x.Type == AuthConstants.UID_KEY).Value;
        }
    }
}
