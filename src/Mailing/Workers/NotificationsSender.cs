using Zbyrach.Api.Account;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Zbyrach.Api.Mailing
{
    public class NotificationsSender : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _smtpUserName;
        private readonly string _smtpPassword;
        private readonly string _smtpHost;

        public NotificationsSender(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _smtpUserName = configuration["SMTP_USERNAME"];
            _smtpPassword = configuration["SMTP_PASSWORD"];
            _smtpHost = configuration["SMTP_HOST"];
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendNotifications(stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task SendNotifications(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mailingSettingsService = scope.ServiceProvider.GetRequiredService<MailingSettingsService>();

            foreach (var settings in await mailingSettingsService.GetScheduledFor(TimeSpan.FromMinutes(60)))
            {
                var email = settings.User.Email;
                var subject = "Your articles from Zbyrach";
                var message = GetMessage(settings.User);
                SendMessage(email, subject, message);
            }
        }

        private string GetMessage(User user)
        {
            return $"Hello {user.Name},";
        }

        private void SendMessage(string to, string subject, string body)
        {
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
