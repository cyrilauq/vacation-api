using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using PusherServer;
using VacationApi.DTO;
using VacationApi.Domains;
using VacationApi.Utils;
using Humanizer;
using Microsoft.AspNetCore.Authorization;

namespace VacationApi.Controllers
{
    [Authorize]
    [Route("tchat")]
    public class TchatController: ControllerBase
    {
        private readonly VacationApiDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TchatController(VacationApiDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Send a message and return a response
        /// </summary>
        /// <param name="vacationId">The vacation's id</param>
        /// <param name="dto">The message information</param>
        /// <returns>
        /// An objectresult with a message
        /// </returns>
        /// <response code="200">The mail sending is successfull</response>
        /// <response code="400">Wrong data are sent or internal server exception</response>
        /// <response code="404">Vacation's not found</response>
        /// <response code="404">User's not found</response>
        [HttpPost]
        [Route("vacation/{vacationId}/message")]
        public async Task<IActionResult> SendMessageAsync(string vacationId, [FromBody] MessageDTO dto)
        {
            var vacation = _context.Vacations.Where(v => v.Id == vacationId).FirstOrDefault();
            if(vacation == null)
            {
                return NotFound("The given id of vacation isn't related to any vacation.");
            }
            var user = _context.Users.Where(u => u.Id == _httpContextAccessor.ConnectedUserId()).FirstOrDefault();
            if (user == null)
            {
                return NotFound("The given id of vacation isn't related to any vacation.");
            }
            if (user.Id != vacation.UserId)
            {
                var invitations = _context.Invitations.Where(i => i.VacationId == vacationId);
                if(!invitations.Any(i => i.UserId == user.Id))
                {
                    return BadRequest("You should be a member of the vacation to post a message inside its tchat.");
                }
            }
            var options = new PusherOptions
            {
                Cluster = "eu",
                Encrypted = true
            };

            var pusher = new Pusher(
              SecretConfig.APP_ID,
              SecretConfig.APP_KEY,
              SecretConfig.APP_SECRET,
              options);

            var result = await pusher.TriggerAsync(
                dto.VacationId,
                "send-message",
                new { 
                    message = dto.Content,
                    uid = user.Id,
                    username = user.UserName,
                    sendDateTime = DateTime.Now
                }
            );

            _context.Messages.Add(
                new Message
                {
                    Content = dto.Content,
                    UserId = user.Id,
                    VacationId = dto.VacationId,
                    SendDateTime = DateTime.Now
                }
            );
            _context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Get all the messages
        /// </summary>
        /// <param name="vacationId">The vacation's id</param>
        /// <returns>
        /// An objectresult with a message
        /// </returns>
        /// <response code="200">The mail sending is successfull</response>
        /// <response code="400">Wrong data are sent or internal server exception</response>
        /// <response code="404">Vacation's not found</response>
        /// <response code="404">User's not found</response>
        [HttpGet]
        [Route("vacation/{vacationId}/message")]
        public IActionResult GetMessage(string vacationId)
        {
            var vacation = _context.Vacations.Where(v => v.Id == vacationId).FirstOrDefault();
            if (vacation == null)
            {
                return NotFound("The given id of vacation isn't related to any vacation.");
            }
            var user = _context.Users.Where(u => u.Id == _httpContextAccessor.ConnectedUserId()).FirstOrDefault();
            if (user == null)
            {
                return NotFound("The given id of vacation isn't related to any vacation.");
            }
            if (user.Id != vacation.UserId)
            {
                var invitations = _context.Invitations.Where(i => i.VacationId == vacationId);
                if (!invitations.Any(i => i.UserId == user.Id))
                {
                    return BadRequest("You should be a member of the vacation to get the messages of its tchat.");
                }
            }
            var messages = _context.Messages.Where(m => m.VacationId == vacationId).ToList().OrderBy(m => m.SendDateTime).TakeLast(100);
            return Ok(messages.Select(m => new
            {
                message = m.Content,
                uid = m.UserId,
                username = _context.Users.Where(v => v.Id == m.UserId).FirstOrDefault().UserName,
                sendDateTime = m.SendDateTime
            }));
        }
    }
}
