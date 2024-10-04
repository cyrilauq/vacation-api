using VacationApi.Domains;
using VacationApi.DTO;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Model;

namespace VacationApi.Infrastructure
{
    public interface IVacationInfrastructure
    {
        Task<IEnumerable<Vacation>> GetVacations();
        Task<IEnumerable<Vacation>> GetVacationsForUser(string uid);
        Task<IEnumerable<UserDTO>> GetUsersForVacation(string vacationId, string userId);
        Vacation? AddVacation(AddVacationModel vacation, String userId);
        Boolean isFree(DateTime from, DateTime to, string userId);
        Boolean ExistsFor(string title, string userId);

        /// <summary>
        /// Return the vacation with the given ID, only if the user is the owner of it.
        /// </summary>
        /// <param name="id">Id of the vacation</param>
        /// <param name="uid">Id the owner of the vacation</param>
        /// <returns></returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="CannotSeeVacationException">when the user cannot see the vacation.</exception>
        Vacation GetVacationByIdFor(String id, string userId);
        Task<IEnumerable<Invitation>> GetInvitations(string id, string userId);

        /// <summary>
        /// Add users to a vacation and retrieve their informations
        /// </summary>
        /// <param name="vacationId">Vacation id where to add members</param>
        /// <param name="userId">Uid of the member that perform the action</param>
        /// <param name="usersUid">Uid of members to add</param>
        /// <returns>
        /// An array of User, wich contains the informations of the added users.
        /// </returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="WrongCredentialsException">when user is not authorized to add members.</exception>
        /// <exception cref="VacationPublishedException">when the vacation is published.</exception>
        /// <exception cref="UserNotFoundException">when one or more of the given uid are not related to any user.</exception>
        List<Invitation> AddUsersToVacation(String vacationId, String userId, String[] usersUid);

        /// <summary>
        /// Publish the given vacation
        /// </summary>
        /// <param name="vacationId">Id of the vacation to add</param>
        /// <returns>
        /// True if the vacation was publish false if not.
        /// </returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="WrongCredentialsException">when user is not authorized to add members.</exception>
        /// <exception cref="VacationPublishedException">when the vacation is published.</exception>
        /// <exception cref="StockageException">when there is a probleme with the storage.</exception>
        Boolean PublishVacation(String vacationId, string userId);

        /// <summary>
        /// Accept the invitation for the given user
        /// </summary>
        /// <param name="invitationId">Id of the invitation</param>
        /// <param name="userId">Id of the user</param>
        /// <returns>
        /// True if the invitation was accepted and the operation successfully ended
        /// </returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="WrongCredentialsException">when user is not authorized to add members.</exception>
        /// <exception cref="VacationPublishedException">when the vacation is published.</exception>
        /// <exception cref="StockageException">when there is a probleme with the storage.</exception>
        Boolean AcceptInvitation(String invitationId, String userId);
    }
}
