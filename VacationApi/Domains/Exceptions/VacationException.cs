namespace VacationApi.Domains.Exceptions
{
    public class InvalidVacationInformation: Exception
    {
        public InvalidVacationInformation(string message): base(message) { }
    }
}
