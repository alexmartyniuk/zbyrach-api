using Zbyrach.Api.Account;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zbyrach.Api.Articles;
using System.Text;
using System.Collections.Generic;

namespace Zbyrach.Api.Mailing
{
    public class NotificationsSender : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly MailService _mailService;
        private readonly int _sendMailsBeforeInMinutes;

        public NotificationsSender(IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            MailService mailService)
        {
            _mailService = mailService;
            _serviceScopeFactory = serviceScopeFactory;
            if (!int.TryParse(configuration["SendMailsBeforeInMinutes"], out _sendMailsBeforeInMinutes))
            {
                _sendMailsBeforeInMinutes = 60;
            }
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

            foreach (var settings in await mailingSettingsService.GetScheduledFor(TimeSpan.FromMinutes(_sendMailsBeforeInMinutes)))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                var articles = await articleService.GetNewForUser(settings.User);

                var email = settings.User.Email;
                var subject = "Your articles from Zbyrach";
                var body = GetMessageBody(settings.User, articles);
                await _mailService.SendMessage(email, subject, body);
            }
        }

        private string GetMessageBody(User user, List<Article> articles)        {
            var body = new StringBuilder();
            body.AppendLine($"Hello {user.Name},");
            body.AppendLine();
            
            foreach (var article in articles)
            {
                body.AppendLine($"{article.Title} ({article.ReadTime})");
                body.AppendLine($"{article.Url}");
                body.AppendLine();
            }

            return body.ToString();
        }
    }
}
