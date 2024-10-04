using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Core.Types;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VacationApi.Auth;
using VacationApi.Domains;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Model;
using VacationApi.Utils;

namespace VacationApi.Services
{
    /// <summary>
    /// Service that will define methods for user manipulations.
    /// </summary>
    public class UsersServices
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUserInfrastructure _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FileService _fileService;
        private readonly IEmailSender _mailsender;

        public UsersServices(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IUserInfrastructure repository,
            IHttpContextAccessor httpContextAccessor,
            FileService fileService,
            IEmailSender mailsender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;
            _mailsender = mailsender;
        }

        /// <summary>
        /// Create a new user based on the given model
        /// </summary>
        /// <param name="model">Model wich contains the informations of the user</param>
        /// <returns>
        /// True is the user has been correctly save.
        /// </returns>
        /// <exception cref="UserAlreadyExistsException">If the user we want to create already exists</exception>
        /// <exception cref="ArgumentException">If an error occured while saving the user</exception>
        public async Task<bool> CreateUser(RegisterModel model)
        {
            // Check User existe déjà avec ce mail
            if (await _userManager.FindByNameAsync(model.Username) != null || _repository.FindUserByEmail(model.Mail) != null)
            {
                throw new UserAlreadyExistsException("The user already exists");
            }

            // Créer le User
            User user = new User
            {
                Name = model.Name,
                Email = model.Mail,
                PicturePath = model.PicturePath,
                UserName = model.Username,
                FirstName = model.Firstname
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                string? message = null;
                foreach (IdentityError error in result.Errors)
                {
                    message += error.Description + " ";
                }
                message ??= "Unkown error occured "; // If errors == null --> errors = "string"
                throw new ArgumentException(message);
            }
            // Ajouter le rôle Identity au User
            var currentUser = await _userManager.FindByNameAsync(model.Username);
            var roleResult = await _userManager.AddToRoleAsync(currentUser, "User");
            if (roleResult.Succeeded)
            {
                if (model.File != null && model.File.Length > 0)
                {
                    currentUser.PicturePath = _fileService.SaveFile(model.File, model.Username, "users");
                    await _userManager.UpdateAsync(currentUser);
                }
            }
            _mailsender.SendEmailAsync(
                currentUser.Email,
                "Inscription onto our vacation manager",
                "Welcome to the best vacation manager ever invented!"
            );
            return true;
        }

        /// <summary>
        /// Search a user and return a DTO whose contains the result of the search.
        /// </summary>
        /// <param name="username">The query</param>
        /// <returns>
        /// 
        /// </returns>
        public UserSearchResultDTO SearchUsers(string username)
        {
            var result = _repository.Search(username, _httpContextAccessor.ConnectedUserId());
            return new UserSearchResultDTO(
                result.Count,
                result.Select(r => new UserSearchResultItemDTO(r.Name, r.FirstName, r.Id, r.Email)).ToArray()
            );
        }

        /// <summary>
        /// Retrieve a user based on the given logs.
        /// </summary>
        /// <param name="login">Username or mail of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>
        /// A DTO wich contains the founded user.
        /// </returns>
        /// <exception cref="UserNotFoundException">If there is no user with the given username/mail and password</exception>
        public async Task<UserDTO?> FindUserByLogs(string login, string password)
        {
            var found = await _repository.GetUserAsync(login, password) ?? throw new UserNotFoundException("The user wasn't found");
            return new UserDTO
            {
                Token = await GenerateToken(found),
                Uid = found.Id,
                Mail = found.Email,
                ImgPath = found.PicturePath,
                Firstname = found.FirstName,
                Name = found.Name,
                Username = found.UserName
            };
        }

        public async Task<UserDTO> LoginWithGoogle(GoogleOAuthTokenDTO token)
        {
            var response = await GoogleJsonWebSignature.ValidateAsync(token.Token);
            if (response.Email == null)
            {
                throw new WrongCredentialsException("The given credentials aren't valid");
            }

            var findResult = _repository.FindUserByEmail(response.Email);
            if (findResult == null)
            {
                User user = new User
                {
                    Name = response.FamilyName,
                    Email = response.Email,
                    PicturePath = response.Picture,
                    UserName = await _repository.ComputeUserName(response.FamilyName, response.GivenName),
                    FirstName = response.GivenName
                };
                var result = await _userManager.CreateAsync(user);
                _mailsender.SendEmailAsync(
                    user.Email,
                    "Inscription onto our vacation manager",
                    "Welcome to the best vacation manager ever invented!"
                );
                return new UserDTO
                {
                    Token = await GenerateToken(user),
                    Uid = user.Id,
                    Mail = user.Email,
                    ImgPath = user.PicturePath,
                    Firstname = user.FirstName,
                    Name = user.Name,
                    Username = user.UserName
                };
            }
            else
            {
                return new UserDTO
                {
                    Token = await GenerateToken(findResult),
                    Uid = findResult.Id,
                    Mail = findResult.Email,
                    ImgPath = findResult.PicturePath,
                    Firstname = findResult.FirstName,
                    Name = findResult.Name,
                    Username = findResult.UserName
                };
            }
        }

        public async Task<List<InvitationDTO>> GetInvitationForConnectedUser()
        {
            return await _repository.GetUserInvitation(_httpContextAccessor.ConnectedUserId());
        }

        /// <summary>
        /// Generate a token for the given user.
        /// </summary>
        /// <param name="user">User for wich we want to create a token</param>
        /// <returns>
        /// A Task that contains the token of the user.
        /// </returns>
        private async Task<string> GenerateToken(User user)
        {
            // Get all user's roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Set the claims of the user
            // like its name, uid and an id for the token
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(AuthConstants.UID_KEY, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            // Add as claims the user's role
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // Create a key encryption
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration.GetValue<string>("JWT:Secret")));
            // Create signing credentials with the generated key
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            // Create a token (JwtSecurityToken)
            var token = new JwtSecurityToken(
                claims: authClaims, // Set the user's claims to the token's claims 
                expires: DateTime.UtcNow.AddHours(1), // Add an expiring date
                audience: _configuration[AuthConstants.AudiencePath], // set the audience
                issuer: _configuration[AuthConstants.IssuerPath], // set the issuer
                signingCredentials: cred // set the signingcredentials
            );
            // Create a string as a token
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
