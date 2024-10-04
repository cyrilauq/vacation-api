using Microsoft.AspNetCore.Identity;
using VacationApi.Domains;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Exceptions;

namespace VacationApi.Repository
{
    public class BDUserRepository : IUserInfrastructure
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly VacationApiDbContext _context;

        public BDUserRepository(
            VacationApiDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public IList<User> Search(string query, string connectedUId)
        {
            var result = _userManager.Users.Where(user => 
                !user.Id.Equals(connectedUId) &&
                ((user.Name + " " + user.FirstName).Contains(query) ||
                (user.FirstName + " " + user.Name).Contains(query) ||
                user.Email.Contains(query) ||
                user.UserName.Contains(query))
            ).ToList();

            return result;
        }

        public async Task<User?> GetUserAsync(string login, string password)
        {
            var user = await _userManager.FindByNameAsync(login);
            if (user == null)
            {
                user = FindUserByEmail(login);
            }
            if(user != null)
            {
                return await _userManager.CheckPasswordAsync(user, password) ? user : throw new WrongCredentialsException("The given credentials are wrong");
            }
            return null;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            var result = await _userManager.FindByIdAsync(id);
            return result != null ? result : throw new UserNotFoundException("The user with the id [" + id + "] wasn't found.");
        }

        public async Task<string> ComputeUserName(string name, string firstname)
        {
            var userCount = _context.Users.Count(u => u.Name == name && u.FirstName == firstname);
            return $"{name}_{firstname}_{userCount}".ToLower();
        }
        public User? FindUserByEmail(string email) 
        {
            return _context.Users.Where(u => u.Email == email).FirstOrDefault();
        }

        public async Task<List<InvitationDTO>> GetUserInvitation(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new UserNotFoundException("The user with the id [" + userId + "] wasn't found.");
            var invitation = _context.Invitations.Where(i => i.UserId == userId).ToList();
            return invitation.Select(i => GetDTO(i)).ToList();
        }

        private InvitationDTO GetDTO(Invitation invitation)
        {
            var vacationName = _context.Vacations.Where(v => v.Id == invitation.VacationId).First().Title;
            return new InvitationDTO
            {
                IsAccepted = invitation.IsAccepted,
                VacationName = vacationName,
                InvitationId = invitation.Id,
                VacationId = invitation.VacationId
            };
        }
    }
}
