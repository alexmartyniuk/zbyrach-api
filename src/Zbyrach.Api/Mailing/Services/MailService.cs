using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Mailing
{
    public class MailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly string _sendGridApiKey;
        private readonly bool _sendMails;
        private readonly string _webUiBasePath;

        public MailService(IConfiguration configuration, ILogger<MailService> logger)
        {
            _logger = logger;
            _sendGridApiKey = configuration["SENDGRID_APIKEY"];
            _sendMails = bool.TrueString.Equals(configuration["SendMails"], StringComparison.OrdinalIgnoreCase);
            _webUiBasePath = configuration["WebUiBasePath"];
        }

        public async Task SendArticleList(User user, string unsubscribeToken, List<Article> articles,
            ScheduleType scheduleType)
        {
            var message = Message.Make(_webUiBasePath, user, unsubscribeToken, articles, scheduleType);
            
            await SendMessage(message);

            _logger.LogInformation($"Message was sent to {message.ToEmail} with articles:\n {articles.Select(a => a.Title + "\n")}");
        }

        private async Task SendMessage(Message message)
        {
            if (!_sendMails)
            {
                Console.WriteLine($"Send '{message.Subject}' to '{message.ToEmail}.'");
                return;
            }

            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress(message.FromEmail, message.FromName);
            var to = new EmailAddress(message.ToEmail, message.ToName);
            var msg = MailHelper.CreateSingleEmail(from, to, message.Subject, string.Empty, message.HtmlBody);
            var response = await client.SendEmailAsync(msg);

            if (!IsSuccessStatusCode(response.StatusCode))
            {
                throw new Exception($"Send Grid returned unsuccessful status code: {response.StatusCode}.");
            }
        }

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return ((int) statusCode >= 200) && ((int) statusCode <= 299);
        }
    }
}