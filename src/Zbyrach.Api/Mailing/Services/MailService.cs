using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scriban;
using SendGrid;
using SendGrid.Helpers.Mail;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing.Templates;

namespace Zbyrach.Api.Mailing
{
    public class MailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly string _sendGridApiKey;
        private readonly bool _sendMails;
        private readonly string _webUiBasePath;
        private readonly Template _articlesEmailTemplate;

        public MailService(IConfiguration configuration, ILogger<MailService> logger)
        {
            _logger = logger;
            _sendGridApiKey = configuration["SENDGRID_APIKEY"];
            _sendMails = bool.TrueString.Equals(configuration["SendMails"], StringComparison.OrdinalIgnoreCase);
            _webUiBasePath = configuration["WebUiBasePath"];

            var templateFileName = Path.Combine(AppContext.BaseDirectory, "Mailing", "Templates", "Articles.html");
            _articlesEmailTemplate = Template.Parse(File.ReadAllText(templateFileName));
        }

        public async Task SendArticleList(User user, string unsubscribeToken, List<Article> articles, ScheduleType scheduleType)
        {
            var subject = "Cтатті за " + GetPeriodInUkrainian(scheduleType);
            var body = GetHtmlMessageBody(user, unsubscribeToken, articles, scheduleType);

            await SendMessage(user, subject, body);

            _logger.LogInformation("Message was sent to {email} with articles:\n {artcileTitles}", user.Email, articles.Select(a => a.Title + "\n"));
        }

        private string GetHtmlMessageBody(User user, string unsubscribeToken, List<Article> articles, ScheduleType scheduleType)
        {
            var baseTemplatesDirectory = Path.Combine(AppContext.BaseDirectory, "Mailing", "Templates");
            unsubscribeToken = WebUtility.UrlEncode(unsubscribeToken);

            var model = new ArticlesModel
            {
                UserName = user.Name,
                UserEmail = user.Email,
                Period = GetPeriodInUkrainian(scheduleType),
                UnsubscribeUrl = $"{_webUiBasePath}/unsubscribe/{unsubscribeToken}",
                ViewOnSiteUrl = $"{_webUiBasePath}/articles/sent",
                Articles = articles.Select(a => new ArticleModel
                {
                    Title = a.Title,
                    Description = a.Description,
                    Url =  $"{_webUiBasePath}/articles/{a.Id}/user/{user.Id}",
                    PublicatedAt = GetDateInUkrainian(a.PublicatedAt),
                    ReadTime = a.ReadTime,
                    PdfUrl = $"{_webUiBasePath}/articles/{a.Id}",
                    AuthorEmail = a.AuthorEmail,
                    AuthorName = a.AuthorName,
                    AuthorPhoto = a.AuthorPhoto
                }).ToList()
            };

            return _articlesEmailTemplate.Render(new { Model = model });
        }

        private string GetPeriodInUkrainian(ScheduleType scheduleType)
        {
            switch (scheduleType)
            {
                case ScheduleType.EveryDay:                    
                    return GetDateInUkrainian(DateTime.UtcNow);
                case ScheduleType.EveryWeek:
                    return "минулий тиждень";
                case ScheduleType.EveryMonth:
                    return "минулий місяць";
                default:
                    throw new ArgumentException(nameof(scheduleType));
            }
        }

        private string GetDateInUkrainian(DateTime date)
        {
            var culture = new CultureInfo("uk-UA");
            return date.ToString("m", culture);
        }

        private async Task SendMessage(User user, string subject, string htmlBody)
        {
            if (!_sendMails)
            {
                Console.WriteLine($"Send '{subject}' to '{user.Email}.'");
                return;
            }

            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("zbyrach@ukr.net", "Збирач");
            var to = new EmailAddress(user.Email, user.Name);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlBody);
            var response = await client.SendEmailAsync(msg);

            if (!IsSuccessStatusCode(response.StatusCode))
            {
                throw new Exception($"Send Grid returned unsuccessful status code: {response.StatusCode}.");
            }
        }

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return ((int)statusCode >= 200) && ((int)statusCode <= 299);
        }
    }
}