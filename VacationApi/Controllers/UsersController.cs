using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using VacationApi.Auth;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VacationApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersServices usersServices;

        public UsersController(UsersServices usersServices)
        {
            this.usersServices = usersServices;
        }

        /// <summary>
        /// Get all the user's that corresponde to the given query
        /// </summary>
        /// <param name="vacationId">The vacation's id</param>
        /// <returns>
        /// An objectresult with a message
        /// </returns>
        /// <response code="200">The mail sending is successfull</response>
        /// <response code="400">Wrong data are sent or internal server exception</response>
        /// <response code="404">Vacation's not found</response>
        /// <response code="404">User's not found</response>
        [Authorize]
        [HttpGet("list/{query}")]
        public IActionResult SearchUsers(string query)
        {
            try
            {
                return Ok(usersServices.SearchUsers(query));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ErrorMessageDTO("An error occured while processing the action"));
            }
        }

        /// <summary>
        /// Get all the invitations of the connected user
        /// </summary>
        /// <returns>
        /// All the invitations of the connected user
        /// </returns>
        /// <response code="200">The invitations are successfully get</response>
        /// <response code="400">The user is not found</response>
        /// <response code="500">Error with the dbcontext</response>
        [Authorize]
        [HttpGet("invitations")]
        public async Task<IActionResult> GetUserInvitationsAsync()
        {
            try
            {
                return Ok(await usersServices.GetInvitationForConnectedUser());
            }
            catch(UserNotFoundException ex)
            {
                return BadRequest(new ErrorMessageDTO(ex.Message));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ErrorMessageDTO("An error occured while processing the action"));
            }
        }
    }
}
