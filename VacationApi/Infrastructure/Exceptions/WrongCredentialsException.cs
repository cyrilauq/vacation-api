namespace VacationApi.Infrastructure.Exceptions
{
    public class WrongCredentialsException: Exception
    {
        public WrongCredentialsException(String message): base(message) { }
    }
}
