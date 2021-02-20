using System;
using System.Collections.Generic;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Mailing
{
    internal class Message
    {
        public string FromEmail { get; }
        public string FromName { get; }
        public string ToEmail { get; }
        public string ToName { get; }
        public string Subject { get; }
        public string HtmlBody { get; }

        public Message(string fromEmail, string fromName, string toEmail, string toName, string subject, string htmlBody)
        {
            FromEmail = fromEmail;
            FromName = fromName;
            ToEmail = toEmail;
            ToName = toName;
            Subject = subject;
            HtmlBody = htmlBody;
        }

        public static Message Make(string webUiBasePath, User user, string unsubscribeToken, List<Article> articles, ScheduleType scheduleType)
        {
            string lang = user.Language ?? "en";
            MessageComposerAbstract composer = lang switch
            {
                "en" => new MesageComposerEnglish(webUiBasePath),
                "uk" => new MesageComposerUkrainian(webUiBasePath),
                _ => throw new NotImplementedException($"Language '{lang}' is not supported.")
            };

            return composer.Compose(user, unsubscribeToken, articles, scheduleType);
        }
    }
}