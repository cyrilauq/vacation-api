using VacationApi.Domains;
using VacationApi.DTO;
using VacationApi.Infrastructure.Exceptions;

namespace VacationApi.Infrastructure
{
    public interface IUserInfrastructure
    {
        /// <summary>
        /// Retrieve a user based on a given login and password.
        /// </summary>
        /// <param name="login">Mail or username of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>
        /// Return a user object if one is found and null if none is found.
        /// </returns>
        Task<User?> GetUserAsync(string login, string password);

        /// <summary>
        /// Allow us to search a user base on a certain query and to exclude one user(the connected one)
        /// </summary>
        /// <param name="query">The given query</param>
        /// <param name="connectedUId">The id of the connected user</param>
        /// <returns>
        /// Returns a collection of user that contains the query inside their fullname, username or email and exclude the connected user from the result
        /// </returns>
        IList<User> Search(string query, string connectedUId);

        /// <summary>
        /// Find and retrieve the user with the given id.
        /// </summary>
        /// <param name="id">Id of the searched user</param>
        /// <returns>
        /// The user related to the id.
        /// </returns>
        Task<User> GetUserByIdAsync(string id);

        /// <summary>
        /// Create a username for the given firstname and name.
        /// </summary>
        /// <param name="name">Name of the user</param>
        /// <param name="firstname">Firstname of the user</param>
        /// <returns>
        /// The new username of the user.
        /// </returns>
        Task<string> ComputeUserName(string name, string firstname);

        /// <summary>
        /// Find the user related to the given email.
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <returns>
        /// A user if one is found and null if none is found.
        /// </returns>
        User? FindUserByEmail(string email);

        /// <summary>
        /// Get all the invitation the user gets.
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>
        /// A list of InvitationDTO that contains all the invitation for the given user.
        /// </returns>
        /// <exception cref="UserNotFoundException">If the given userId isn't related to any existing user</exception>
        Task<List<InvitationDTO>> GetUserInvitation(string userId);
    }
}
