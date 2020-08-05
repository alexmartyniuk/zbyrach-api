using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RazorLight;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing.Templates;

namespace Zbyrach.Api.Mailing
{
    public class MailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly string _smtpUserName;
        private readonly string _smtpPassword;
        private readonly string _smtpHost;
        private readonly bool _sendMails;
        private readonly string _apiBasePath;
        private readonly string _uiBasePath;

        public MailService(IConfiguration configuration, ILogger<MailService> logger)
        {
            _logger = logger;
            _smtpUserName = configuration["SMTP_USERNAME"];
            _smtpPassword = configuration["SMTP_PASSWORD"];
            _smtpHost = configuration["SMTP_HOST"];
            _sendMails = bool.TrueString.Equals(configuration["SendMails"], StringComparison.OrdinalIgnoreCase);
            _apiBasePath = configuration["ApiBasePath"];
            _uiBasePath = configuration["UiBasePath"];
        }

        public async Task SendArticleList(User user, string unsubscribeToken, List<Article> articles)
        {
            var subject = "Cтатті від Збирача за " + GetCurrentDateInUkrainian();
            var body = await GetHtmlMessageBody(user, unsubscribeToken, articles);
            SendMessage(user.Email, subject, body);
            _logger.LogInformation("Message was sent to {email} with articles:\n {artcileTitles}", user.Email, articles.Select(a => a.Title + "\n"));
        }
        
        private async Task<string> GetHtmlMessageBody(User user, string unsubscribeToken, List<Article> articles)
        {
            var baseTemplatesDirectory = Path.Combine(AppContext.BaseDirectory, "Mailing", "Templates");
            unsubscribeToken = WebUtility.UrlEncode(unsubscribeToken);
                
            var model = new ArticlesModel
            {
                UserName = user.Name,
                UserEmail = user.Email,
                DateTime = GetCurrentDateInUkrainian(),
                UnsubscribeUrl = $"{_uiBasePath}/unsubscribe/{unsubscribeToken}",
                ViewOnSiteUrl =  $"{_uiBasePath}/articles",
                Articles = articles.Select(a => new ArticleModel
                {
                    Title = a.Title,
                    Description = a.Description,
                    Url = a.Url,
                    PdfUrl = $"{_apiBasePath}/articles/pdf/{a.Id}",
                    AuthorEmail = a.AuthorEmail,
                    AuthorName = a.AuthorName,
                    AuthorPhoto = a.AuthorPhoto
                }).ToList()
            };

            // TODO: optimize this with cache
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(baseTemplatesDirectory)
                .UseMemoryCachingProvider()
                .Build();

            return await engine.CompileRenderAsync("Articles.cshtml", model);
        }

        private string GetCurrentDateInUkrainian()
        {
            var culture = new CultureInfo("uk-UA");
            return DateTime.UtcNow.ToString("m", culture);
        }

        private void SendMessage(string to, string subject, string htmlBody)
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
                Body = htmlBody,
                IsBodyHtml = true,
                Subject = subject,
            };
            mailMessage.To.Add(to);
            client.Send(mailMessage);
        }
    }
}