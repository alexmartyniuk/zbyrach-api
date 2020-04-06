using Zbyrach.Api.Account;
using System;

namespace Zbyrach.Api.Mailing
{
    public class MailingSettings
    {
        public long Id { get; set; }
        public string Schedule { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastSentAt { get; set; }
        public long NumberOfArticles { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}