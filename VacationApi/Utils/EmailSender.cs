using System.Net.Mail;
using System.Net;
using VacationApi.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;

namespace VacationApi.Utils
{
    public class EmailSender : IEmailSender
    {
        public void SendEmailAsync(string mail, string subject, string message)
        {
            MimeMessage email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(SecretConfig.ADMIN_MAIL));
            email.To.Add(MailboxAddress.Parse(mail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = message
            };

            ISmtpClient smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(SecretConfig.ADMIN_MAIL, SecretConfig.ADMIN_PASSWORD);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
