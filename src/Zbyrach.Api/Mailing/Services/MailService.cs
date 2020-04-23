using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Zbyrach.Api.Mailing
{
    public class MailService
    {
        private readonly string _smtpUserName;
        private readonly string _smtpPassword;
        private readonly string _smtpHost;
        private readonly bool _sendMails;

        public MailService(IConfiguration configuration)
        {
            _smtpUserName = configuration["SMTP_USERNAME"];
            _smtpPassword = configuration["SMTP_PASSWORD"];
            _smtpHost = configuration["SMTP_HOST"];
            _sendMails = bool.TrueString.Equals(configuration["SendMails"], StringComparison.OrdinalIgnoreCase);
        }

        public async Task SendMessage(string to, string subject, string body)
        {
            if (!_sendMails)
            {
                Console.WriteLine($"Send '{subject}' to '{to}.'");
                return;
            }

            var client = new SmtpClient(_smtpHost)
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUserName, _smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUserName),
                Body = body,
                Subject = subject,
            };
            mailMessage.To.Add(to);
            client.Send(mailMessage);
        }
    }
}