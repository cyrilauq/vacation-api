namespace VacationApi.Infrastructure.Exceptions
{
    public class CannotSeeVacationException : Exception
    {
        public CannotSeeVacationException(String message) : base(message) { }
    }

    public class VacationNotFoundException : Exception
    {
        public VacationNotFoundException(String message) : base(message) { }
    }

    public class VacationAlreadyExistsException : Exception
    {
        public VacationAlreadyExistsException(String message) : base(message) { }
    }

    public class VacationPublishedException: Exception
    {
        public VacationPublishedException(String message) : base(message) { }
    }
}
