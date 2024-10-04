using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;
using Ical.Net.Collections;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VacationApi.Domains;
using VacationApi.Domains.Exceptions;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Args;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Services;

namespace VacationApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VacationActivitiesController : ControllerBase
    {
        private readonly ActivitiesServices _activitiesServices;

        public VacationActivitiesController(ActivitiesServices activitiesServices)
        {
            _activitiesServices = activitiesServices;
        }

        // GET: /api/vacation/activity
        /// <summary>
        /// Get activities linked to a vacation
        /// </summary>
        /// <param name="id">The vacation's id</param>
        /// <returns>
        /// An objectresult with a all the activities of the given vacation.
        /// </returns>
        /// <response code="200">The activities of the given vacation</response>
        /// <response code="400">Vacation not found</response>
        /// <response code="401">User can't see the vacation</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [Route("/vacation/{id}/activities")]
        public ActionResult<IEnumerable<VacationActivity>> GetVacationActivities(string id)
        {
            try
            {
                return Ok(_activitiesServices.GetActivitiesForVacation(id));
            }
            catch (Exception ex)
            {
                if (ex is VacationNotFoundException)
                {
                    return BadRequest(ex.Message);
                }
                if (ex is WrongCredentialsException)
                {
                    return Unauthorized(ex.Message);
                }
                return StatusCode(500, "Erreur serveur interne");
            }
        }

        /// <summary>
        /// Get the ics file that contains the vacations planning
        /// </summary>
        /// <param name="id">The vacation's id</param>
        /// <returns>
        /// An objectresult with a the planning file.
        /// </returns>
        /// <response code="200">The activities of the given vacation</response>
        /// <response code="400">Vacation not found</response>
        /// <response code="401">User can't see the vacation</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [Authorize]
        [Route("/vacation/{id}/planning/ics")]
        public IActionResult DownloadPlanning(string id)
        {
            try
            {
                var serializer = new CalendarSerializer();
                var iscFile = serializer.SerializeToString(_activitiesServices.GetCalendarForVacation(id));

                // Set the Content-Disposition header to make the file downloadable
                var contentDisposition = new ContentDisposition
                {
                    FileName = id + ".ics",
                    Inline = false  // Set to true if you want the browser to attempt to display the file inline
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return new FileContentResult(Encoding.ASCII.GetBytes(iscFile), "text/calendar");
            }
            catch (Exception ex)
            {
                if (ex is VacationNotFoundException)
                {
                    return BadRequest(ex.Message);
                }
                if (ex is WrongCredentialsException)
                {
                    return Unauthorized(ex.Message);
                }
                return StatusCode(500, "Erreur serveur interne");
            }
        }

        /// <summary>
        /// Get the all the plannified activities of the vacation.
        /// </summary>
        /// <param name="id">The vacation's id</param>
        /// <returns>
        /// An objectresult with a the planning file.
        /// </returns>
        /// <response code="200">The plannified activities of the given vacation</response>
        /// <response code="400">Vacation not found or User can't see the vacation</response>
        /// <response code="500">Internal server error</response>

        [HttpGet]
        [Authorize]
        [Route("/vacation/{id}/planning")]
        public ActionResult<IEnumerable<ActivityDTO>> GetPlanning(string id)
        {
            try
            {
                return Ok(_activitiesServices.GetPlanningForVacation(id));
            }
            catch (Exception ex)
            {
                if (ex is VacationNotFoundException || ex is WrongCredentialsException || ex is CannotSeeVacationException)
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(500, "Erreur serveur interne");
            }
        }

        /// <summary>
        /// Plannify an activity
        /// </summary>
        /// <param name="activityId">The activity's id</param>
        /// <param name="dto">The plannification informations</param>
        /// <returns>
        /// An objectresult with a the planning file.
        /// </returns>
        /// <response code="200">The plannified activities of the given vacation</response>
        /// <response code="400">Wrong data sent</response>
        /// <response code="403">User can't see the vacation</response>
        /// <response code="404">Vacation not found</response>
        [Authorize]
        [HttpPut]
        [Route("{activityId}/planning")]
        public IActionResult PlannifyActivity(string activityId, [FromBody] PlannifyActivityDTO dto)
        {
            try
            {
                return Ok(_activitiesServices.PlannifyActivity(activityId, dto));
            }
            catch (Exception e) when(e is WrongCredentialsException || e is CannotSeeVacationException)
            {
                return StatusCode(((int)HttpStatusCode.Forbidden));
            }
            catch (ActivityNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e) when (e is VacationPublishedException || e is InvalidVacationInformation || e is PeriodNotFreeException || e is CannotSeeVacationException)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Give an array of activities to add the given id
        /// </summary>
        /// <param name="body">The activity's id</param>
        /// <returns>
        /// An objectresult with a the planning file.
        /// </returns>
        /// <response code="200">The added activities of the given vacation</response>
        /// <response code="400">Wrong data sent</response>
        /// <response code="403">User can't see the vacation</response>
        /// <response code="404">Vacation not found</response>
        [HttpPost]
        [Authorize]
        public IActionResult AddActivities([FromBody] ActivitiesDTO body)
        {
            try
            {
                return Ok(_activitiesServices.AddActivitiesToVacation(body.VacationId, body.activities));
            }
            catch (VacationActivitySaveExceptions ex)
            {
                return StatusCode(500, new ErrorMessageDTO(ex.Message));
            }
        }
    }
}
