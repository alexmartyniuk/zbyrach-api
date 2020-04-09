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
using Zbyrach.Api.Articles;
using System.Text;

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
            var articleService = scope.ServiceProvider.GetRequiredService<ArticleService>();

            foreach (var settings in await mailingSettingsService.GetScheduledFor(TimeSpan.FromMinutes(60)))
            {
                var email = settings.User.Email;
                var subject = "Your articles from Zbyrach";
                var body = await GetMessageBody(settings.User, articleService);
                SendMessage(email, subject, body);
            }
        }

        private async Task<string> GetMessageBody(User user, ArticleService articleService)
        {
            var body = new StringBuilder();
            body.AppendLine($"Hello {user.Name},");
            body.AppendLine();

            var articles = await articleService.GetAllForUser(user); 
            foreach (var article in articles)
            {
                body.AppendLine($"{article.Title} ({article.ReadTime})");
                body.AppendLine($"{article.Url}");
                body.AppendLine();
            }

            return body.ToString();
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
