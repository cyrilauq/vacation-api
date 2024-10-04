using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationApi.Auth;
using VacationApi.Domains;
using VacationApi.Domains.Exceptions;
using VacationApi.DTO.Vacation;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Data.SqlTypes;
using VacationApi.Services;
using Humanizer;

namespace VacationApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VacationsController : ControllerBase
    {
        private readonly VacationsServices _vacationsServices;

        public VacationsController(VacationsServices vacationsServices)
        {
            _vacationsServices = vacationsServices;
        }

        // GET: api/Vacations/ByCountriesNbUsers
        /// <summary>
        /// Get the number of users in each places that are in vacation for the given date
        /// </summary>
        /// <param name="date">Format : dd-MM-yyyy. Other formats won't always return an error but will return irrelevant data</param>
        /// <returns>
        /// An objectresult with a dictionnary with the country and the number of user for it.
        /// </returns>
        /// <response code="200">The statistics of the given vacation</response>
        /// <response code="400">Wrong data sent</response>
        [HttpGet("CountriesStatistics/{date}")]
        public ActionResult<Dictionary<string, int>> GetNumberOfUserByPlace(string date)
        {
            try
            {
                return Ok(_vacationsServices.GetUserByPlaceForDate(date));
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get the members of the given vacation
        /// </summary>
        /// <param name="vacationId">Id of the vacation</param>
        /// <returns>
        /// An objectresult all the user related to the vacation
        /// </returns>
        /// <response code="200">The members of the given vacation</response>
        /// <response code="400">Wrong data sent</response>
        /// <response code="403">User can't see the vacation</response>
        [HttpGet("{vacationId}/Members")]
        [Authorize]
        public async Task<ActionResult<List<UserSearchResultItemDTO>>> GetUsers(string vacationId)
        {

            try
            {
                return Ok(await _vacationsServices.GetUsersForVacation(vacationId));
            }
            catch (VacationNotFoundException e)
            {
                return BadRequest(new ErrorMessageDTO("The given Id doesn't match any vacation"));
            }
            catch (CannotSeeVacationException e)
            {
                return Unauthorized(new ErrorMessageDTO("The user cannot see the vacation"));
            }
        }

        /// <summary>
        /// Get the vacation with the given id
        /// </summary>
        /// <param name="id">The id of the vacation</param>
        /// <returns>
        /// An objectresult with the vacation.
        /// </returns>
        /// <remarks>
        /// 
        /// Sample request : GET Vacations/5
        /// 
        /// </remarks>
        /// <response code="200">The wanted vacation</response>
        /// <response code="400">the given id isn't related to any vacation</response>
        /// <response code="403">User can't see the vacation</response>
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult GetVacation(string id)
        {
            try
            {
                return Ok(_vacationsServices.GetVacationById(id));
            } 
            catch(VacationNotFoundException e)
            {
                return BadRequest(new ErrorMessageDTO("The given Id doesn't match any vacation"));
            }
            catch(CannotSeeVacationException e)
            {
                return Unauthorized(new ErrorMessageDTO("The user cannot see the vacation"));
            }
        }

        /// <summary>
        /// Update the vacation with the given id
        /// </summary>
        /// <param name="id">The id of the vacation</param>
        /// <param name="dto">The informations of the vacation</param>
        /// <returns>
        /// An objectresult with a dictionnary with the country and the number of user for it.
        /// </returns>
        /// <response code="200">The activities of the given vacation</response>
        /// <response code="400">the given id isn't related to any vacation or user can't see it or vacation cannot be updated</response>
        [Authorize]
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutVacation(string id, [FromBody] VacationDTO dto)
        {
            try
            {
                var result = _vacationsServices.PublishVacation(id);
                return Ok();
            }
            catch (Exception e) when (e is VacationNotFoundException || e is WrongCredentialsException || e is VacationPublishedException || e is StockageException)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get the vacations related to the connected user
        /// </summary>
        /// <returns>
        /// An objectresult with a list with the vacation of the user.
        /// </returns>
        /// <remarks>
        /// 
        /// Sample request
        /// GET /Vacations/User
        /// 
        /// </remarks>
        /// <response code="200">The vacation of the user</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [Route("User")]
        [Authorize]
        public async Task<IActionResult> GetVacationsForUser()
        {
            try
            {
                return Ok(await _vacationsServices.GetCurrentUserVacation());
            } catch (DbUpdateException)
            {
                return StatusCode(500, "An internal server error occured while trying to fetch the vacations");
            }
        }

        // POST: api/Vacations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Adds a vacation
        /// </summary>
        /// <param name="vacation">Information of the vacation</param>
        /// <returns>
        /// An objectresult with the informations of the added vacation.
        /// </returns>
        /// <response code="200">The activities of the given vacation</response>
        /// <response code="500">the given id isn't related to any vacation or user can't see it or vacation cannot be updated</response>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Vacation([FromForm] AddVacationModel vacation)
        {
            try
            {
                return Ok(_vacationsServices.AddVacation(vacation));
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, new ErrorMessageDTO("An internal server error occured while trying to add the vacation."));
                // return BadRequest(new AddVacationDto(false, "An internal server error occured while trying to add the vacation."));
            }
            catch (Exception e) when (e is VacationAlreadyExistsException || e is InvalidVacationInformation || e is PeriodNotFreeException)
            {
                return StatusCode(500, new ErrorMessageDTO("An error occured: " + e.Message));
            }
        }

        /// <summary>
        /// Add a member to the vacation
        /// </summary>
        /// <param name="dto">A dto with the vacationid and the user uid that will be added</param>
        /// <returns>
        /// An objectresult with all the added user.
        /// </returns>
        /// <response code="200">The added user</response>
        /// <response code="400">Wrong data sent or User can't see the vacation or Vacation not found</response>
        [HttpPost]
        [Route("members")]
        [Authorize]
        public async Task<IActionResult> AddMemberToVacationAsync([FromBody] AddMembersDto dto)
        {
            try
            {
                return Ok(await _vacationsServices.AddMembersToVacation(dto));
            }
            catch (VacationNotFoundException ex)
            {
                return BadRequest(new ErrorMessageDTO("Vacation wans't found."));
            }
            catch (UserNotFoundException ex)
            {
                return BadRequest(new ErrorMessageDTO("One of the given users doesn't exist."));
            }
            catch (WrongCredentialsException ex)
            {
                return BadRequest(new ErrorMessageDTO("You can't perform this action."));
            }
            catch (VacationPublishedException ex)
            {
                return BadRequest(new ErrorMessageDTO("This vacation is published wich means you can't add member."));
            }
        }
        /// <summary>
        /// Update the information of the invitation
        /// </summary>
        /// <param name="vacationId">id of the vacation</param>
        /// <param name="invitationId">id of the invitation</param>
        /// <param name="isAccepted">dto with the invitation information's</param>
        /// <returns>
        /// An objectresult with a dictionnary with the country and the number of user for it.
        /// </returns>
        /// <response code="200">The activities of the given vacation</response>
        /// <response code="400">Wrong data sent or vacation cannot be updated or internal error</response>
        /// <response code="403">User can't see the vacation</response>
        /// <response code="404">Vacation not found</response>
        [HttpPut]
        [Route("{vacationId}/invitation/{invitationId}")]
        [Authorize]
        public async Task<IActionResult> AcceptInvitation(string vacationId, string invitationId, [FromBody] InvitationDTO isAccepted)
        {
            try
            {
                return Ok(_vacationsServices.AcceptInvitation(invitationId));
            }
            catch (VacationNotFoundException ex)
            {
                return NotFound(new ErrorMessageDTO("Vacation wans't found."));
            }
            catch (WrongCredentialsException ex)
            {
                return Unauthorized(new ErrorMessageDTO("You can't perform this action."));
            }
            catch (VacationPublishedException ex)
            {
                return BadRequest(new ErrorMessageDTO("This vacation is published wich means you can't add member."));
            }
            catch (StockageException ex)
            {
                return BadRequest(new ErrorMessageDTO(ex.Message));
            }
        }
    }
}
