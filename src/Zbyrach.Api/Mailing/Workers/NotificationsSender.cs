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
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Zbyrach.Api.Mailing
{
    public class NotificationsSender : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly MailService _mailService;
        private readonly ILogger<NotificationsSender> _logger;
        private readonly int _sendMailsBeforeInMinutes;

        public NotificationsSender(IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            MailService mailService,
            ILogger<NotificationsSender> logger)
        {
            _mailService = mailService;
            _logger = logger;
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
                    await SendEmails(stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task SendEmails(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mailingSettingsService = scope.ServiceProvider.GetRequiredService<MailingSettingsService>();
            var articleService = scope.ServiceProvider.GetRequiredService<ArticleService>();

            foreach (var settings in await mailingSettingsService.GetBySchedule(TimeSpan.FromMinutes(_sendMailsBeforeInMinutes)))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await SendEmail(articleService, settings);
            }
        }

        private async Task SendEmail(ArticleService articleService, MailingSettings settings)
        {
            var articles = await articleService.GetForSending(settings.User, settings.NumberOfArticles);
            if (articles.Count == 0)
            {
                _logger.LogInformation("No articles to send for {email}", settings.User.Email);
                return;
            }

            var subject = "Your articles from Zbyrach";
            var body = GetMessageBody(settings.User, articles);
            _mailService.SendMessage(settings.User.Email, subject, body);
            _logger.LogInformation("Message was sent to {email} with articles:\n {artcileTitles}", settings.User.Email, articles.Select(a => a.Title + "\n"));

            await articleService.MarkAsSent(articles, settings.User);
        }

        private string GetMessageBody(User user, List<Article> articles)
        {
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
