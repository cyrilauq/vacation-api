namespace VacationApi.Infrastructure.Exceptions
{
    public class PeriodNotFreeException : Exception
    {
        public PeriodNotFreeException(String message) : base(message) { }
    }

    /// <summary>
    /// Represent an exceptions that can be thrown when there is an exception during the utilisation of the storage ressources
    /// </summary>
    public class StockageException: Exception
    {
        public StockageException(String message) : base(message) { }
    }
}
