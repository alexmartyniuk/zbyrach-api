using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zbyrach.Api.Articles;
using Microsoft.Extensions.Logging;
using Zbyrach.Api.Account;

namespace Zbyrach.Api.Mailing
{
    public class NotificationsSender : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly MailService _mailService;
        private readonly CronService _cronService;
        private readonly ILogger<NotificationsSender> _logger;
        private readonly int _sendMailsBeforeInMinutes;

        public NotificationsSender(IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            MailService mailService,
            CronService cronService,
            ILogger<NotificationsSender> logger)
        {
            _mailService = mailService;
            _cronService = cronService;
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

            foreach (var settings in await mailingSettingsService.GetBySchedule(TimeSpan.FromMinutes(_sendMailsBeforeInMinutes)))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await SendEmail(scope, settings);
            }
        }

        private async Task SendEmail(IServiceScope serviceScope, MailingSettings settings)
        {
            var articleService = serviceScope.ServiceProvider.GetRequiredService<ArticleService>();
            var articles = await articleService.GetForSending(settings.User, settings.NumberOfArticles);
            if (articles.Count == 0)
            {
                _logger.LogInformation("No articles to send for {email}", settings.User.Email);
                return;
            }

            var usersService = serviceScope.ServiceProvider.GetRequiredService<UsersService>();
            var unsubscribeToken = usersService.GetUnsubscribeTokenByUser(settings.User);

            await _mailService.SendArticleList(settings.User, unsubscribeToken, articles, _cronService.ExpressionToSchedule(settings.Schedule));            
            await articleService.MarkAsSent(articles, settings.User);
        }
    }
}
