namespace VacationApi.Infrastructure.Exceptions
{

    public class ActivityAlreadyExistException : Exception
    {
        public ActivityAlreadyExistException(String message) : base(message) { }
    }

    public class ActivityNotFoundException : Exception
    {
        public ActivityNotFoundException(String message) : base(message) { }
    }
}
