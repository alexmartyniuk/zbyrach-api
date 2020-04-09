using Zbyrach.Api.Account;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

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

            foreach (var settings in await mailingSettingsService.GetScheduledFor(TimeSpan.FromMinutes(_sendMailsBeforeInMinutes)))
            {
                var email = settings.User.Email;
                var subject = "Your articles from Zbyrach";
                var message = GetMessage(settings.User);
                await _mailService.SendMessage(email, subject, message);
            }
        }

        private string GetMessage(User user)
        {
            return $"Hello {user.Name},";
        }       
    }
}
