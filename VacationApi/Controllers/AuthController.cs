using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using VacationApi.Domains;
using VacationApi.DTO;
using VacationApi.Model;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Services;
using System.Net;
using FluentFTP;
using Microsoft.AspNetCore.Rewrite;
using Google.Apis.PeopleService.v1.Data;
using System.Net.Sockets;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace VacationApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly UsersServices usersServices;

        public AuthController(
            UserManager<User> userManager,
            UsersServices usersServices)
        {
            _userManager = userManager;
            this.usersServices = usersServices;
        }

        /// <summary>
        /// Log in the user
        /// </summary>
        /// <param name="model">The user's credentials</param>
        /// <returns>
        /// A UserDTO with all the user's informations
        /// </returns>
        /// <response code="200">Login is successfull</response>
        /// <response code="400">Wrong data are sent</response>
        /// <response code="401">Wrong data are sent</response>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                return Ok(await usersServices.FindUserByLogs(model.UserName, model.Password));
            }
            catch(UserNotFoundException ex)
            {
                return BadRequest(new ErrorMessageDTO("Vérifiez votre login"));
            }
            catch(WrongCredentialsException ex)
            {
                return Unauthorized(new ErrorMessageDTO("Vérifiez votre mot de passe"));
            }
        }

        /// <summary>
        /// Register the user
        /// </summary>
        /// <param name="model">The user's informations</param>
        /// <returns>
        /// An objectresult with a message
        /// </returns>
        /// <response code="200">Register is successfull</response>
        /// <response code="400">Wrong data are sent</response>
        /// <response code="409">User already exists</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            try
            {
                return await usersServices.CreateUser(model) ? Ok(new RegisterDTO { message = "Utilisateur créer avec succès" }) : StatusCode(500, new ErrorMessageDTO("Erreur interne du serveur API"));
            }
            catch(UserAlreadyExistsException e)
            {
                return Conflict(e.Message);
            }
            catch(ArgumentException e)
            {
                return BadRequest(new ErrorMessageDTO(e.Message));
            }
        }

        /// <summary>
        /// Register or log in the user with google
        /// </summary>
        /// <param name="token">An object with google token</param>
        /// <returns>
        /// A UserDTO with all the user's informations or an ObjectResult if something gone wrong.
        /// </returns>
        /// <response code="200">Register or login is successfull</response>
        /// <response code="400">Wrong data are sent</response>
        [HttpPost]
        [Route("google")]
        public async Task<IActionResult> authWithGoogle([FromBody] GoogleOAuthTokenDTO token)
        {
            try
            {
                return Ok(await usersServices.LoginWithGoogle(token));
            }
            catch(WrongCredentialsException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
