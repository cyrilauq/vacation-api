using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using VacationApi.DTO;
using VacationApi.Services;
using MimeKit;
using MimeKit.Text;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace VacationApi.Controllers
{
    [Route("mail")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        public ContactController(IEmailSender emailSender) 
        {
            _emailSender = emailSender;
        }

        /// <summary>
        /// Send a mail and return a response
        /// </summary>
        /// <param name="body">The mail informations</param>
        /// <returns>
        /// An objectresult with a message
        /// </returns>
        /// <response code="200">The mail sending is successfull</response>
        /// <response code="400">Wrong data are sent or internal server exception</response>
        [HttpPost]
        public async Task<IActionResult> SendMailAsync([FromBody] MailDTO body)
        {
            if (body.SenderFirstName.Trim().Length == 0)
            {
                return BadRequest("The sender firstname shouldn't be empty.");
            }
            if (body.SenderName.Trim().Length == 0)
            {
                return BadRequest("The sender name shouldn't be empty.");
            }
            if (body.SenderMail.Trim().Length == 0)
            {
                return BadRequest("The sender mail shouldn't be empty.");
            }
            if (body.Message.Trim().Length == 0)
            {
                return BadRequest("The message shouldn't be empty.");
            }

            try
            {
                // Send confirmation mail
                _emailSender.SendEmailAsync(
                    body.SenderMail, 
                    "Admin Contact", 
                    "Hello " + body.SenderFirstName + ",\nWe have received your message, we will read it as soon as possible and come bakc to you"
                );
                // Send mail to admin
                _emailSender.SendEmailAsync(SecretConfig.ADMIN_MAIL, "Admin Contact", "Message from " + body.SenderFirstName + " " + body.SenderName + " (" + body.SenderMail + ").\r\n Contenu: " + body.Message);

                // Envoyer une réponse JSON avec succès
                return Ok(new { success = true, message = "Message transmis" });
            }
            catch (Exception ex)
            {
                // Envoyer une réponse JSON avec erreur
                return BadRequest(new { success = false, message = "Une erreur s'est produite lors de l'envoi du message : " + ex.Message });
            }
        }
    }
}
