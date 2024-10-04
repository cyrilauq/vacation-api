using VacationApi.Domains;
using VacationApi.Domains.Exceptions;
using VacationApi.DTO;
using VacationApi.Infrastructure.Args;
using VacationApi.Infrastructure.Exceptions;

namespace VacationApi.Infrastructure
{
    public interface IVacationActivitiesInfrastructure
    {
        /// <summary>
        /// Add activites to the given vacation.
        /// </summary>
        /// <param name="vacationId">Id of the vacation</param>
        /// <param name="activities">Activities to add to the vacation</param>
        /// <returns>
        /// Return true if the method correctly end.
        /// </returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="WrongCredentialsException">when user is not authorized to add activities.</exception>
        /// <exception cref="VacationPublishedException">when the vacation is published.</exception>
        /// <exception cref="UserNotFoundException">when the given uid is not related to any user.</exception>
        List<VacationActivity> AddActivitiesToVacation(String vacationId, List<ActivityDTO> activities, string userId);

        /// <summary>
        /// Return the activites to the given vacation.
        /// </summary>
        /// <param name="vacationId">Id of the vacation</param>
        /// <returns>
        /// Return true if the method correctly end.
        /// </returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="WrongCredentialsException">when user is not authorized to see the vacation.</exception>
        /// <exception cref="UserNotFoundException">when the given uid is not related to any user.</exception>
        List<VacationActivity> GetActivitiesForVacation(String vacationId, string userId);

        /// <summary>
        /// Plannify a given activity to the given time
        /// </summary>
        /// <param name="args">Args to give for the plannification of the activity</param>
        /// <returns>
        /// Return the plannified activity
        /// </returns>
        /// <exception cref="ActivityNotFoundException">The activity wasn't found.</exception>
        /// <exception cref="VacationPublishedException">when the vacation is published.</exception>
        /// <exception cref="UserNotFoundException">when the user that make the request isn't found.</exception>
        /// <exception cref="PeriodNotFreeException">when the given period isn't free.</exception>
        /// <exception cref="InvalidVacationInformation">when one or multiple information isn't valid.</exception>
        /// <exception cref="WrongCredentialsException">when the connected user doesn't own the vacation the activity is related to.</exception>
        VacationActivity PlannifyActivity(PlannifyArgs args, string userId);
    }
}
