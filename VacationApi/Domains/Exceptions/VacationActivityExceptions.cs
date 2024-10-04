namespace VacationApi.Domains.Exceptions
{
    public class VacationActivityExceptions : Exception
    {
        public VacationActivityExceptions(string message) : base(message) { }
    }

    public class VacationActivityFormatExceptions : VacationActivityExceptions
    {
        public VacationActivityFormatExceptions(string message) : base(message) { }
    }

    public class VacationActivitySaveExceptions : VacationActivityExceptions
    {
        public VacationActivitySaveExceptions(string message) : base(message) { }
    }
}
