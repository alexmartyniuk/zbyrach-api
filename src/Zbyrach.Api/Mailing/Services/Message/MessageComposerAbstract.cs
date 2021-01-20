using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Scriban;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing.Templates;

namespace Zbyrach.Api.Mailing
{
    internal abstract class MessageComposerAbstract
    {
        private readonly string _webUiBasePath;

        protected MessageComposerAbstract(string webUiBasePath)
        {
            _webUiBasePath = webUiBasePath;
        }

        public Message Compose(User user, string unsubscribeToken, List<Article> articles,
            ScheduleType scheduleType)
        {
            var templateFileName = Path.Combine(AppContext.BaseDirectory, "Mailing", "Templates", GetTemplateFileName());
            var articlesEmailTemplate = Template.Parse(File.ReadAllText(templateFileName));

            var result = new Message
            {
                Subject = GetTitle() + " " + GetPeriod(scheduleType),
                HtmlBody = GetHtmlMessageBody(user, unsubscribeToken, articles, scheduleType, articlesEmailTemplate),
                FromEmail = "zbyrach@ukr.net",
                FromName = GetFromName(),
                ToEmail = user.Email,
                ToName = user.Name
            };

            return result;
        }

        private string GetHtmlMessageBody(User user, string unsubscribeToken, List<Article> articles,
            ScheduleType scheduleType, Template articlesEmailTemplate)
        {
            unsubscribeToken = WebUtility.UrlEncode(unsubscribeToken);

            var model = new ArticlesModel
            {
                UserName = user.Name,
                UserEmail = user.Email,
                Period = GetPeriod(scheduleType),
                UnsubscribeUrl = $"{_webUiBasePath}/unsubscribe/{unsubscribeToken}",
                ViewOnSiteUrl = $"{_webUiBasePath}/articles/sent",
                Articles = articles.Select(a => new ArticleModel
                {
                    Title = a.Title,
                    Description = a.Description,
                    Url = $"{_webUiBasePath}/articles/{a.Id}/user/{user.Id}",
                    PublicatedAt = GetDate(a.PublicatedAt),
                    ReadTime = a.ReadTime,
                    PdfUrl = $"{_webUiBasePath}/articles/{a.Id}",
                    AuthorEmail = a.AuthorEmail,
                    AuthorName = a.AuthorName,
                    AuthorPhoto = a.AuthorPhoto
                }).ToList()
            };

            return articlesEmailTemplate.Render(new { Model = model });
        }

        protected abstract string GetFromName();
        protected abstract string GetTitle();
        protected abstract string GetTemplateFileName();
        protected abstract string GetDate(in DateTime date);
        protected abstract string GetPeriod(ScheduleType scheduleType);
    }
}