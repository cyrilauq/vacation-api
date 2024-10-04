using VacationApi.Domains;
using VacationApi.Infrastructure.Exceptions;

namespace VacationApi.Infrastructure
{
    public interface IVacationGetter
    {
        /// <summary>
        /// Return the vacation with the given ID, only if the user is the owner of it.
        /// </summary>
        /// <param name="id">Id of the vacation</param>
        /// <param name="uid">Id the owner of the vacation</param>
        /// <returns>
        /// Null if the vacation wasn't found
        /// A vacation otherwise
        /// </returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="CannotSeeVacationException">when the user cannot see the vacation.</exception>
        Vacation? GetVacationById(string id, string userId);
    }
}
