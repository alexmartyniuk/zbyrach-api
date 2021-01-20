using System;
using System.Collections.Generic;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Mailing
{
    internal class Message
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ToEmail { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }

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