using MediumGrabber.Api.Account;

namespace MediumGrabber.Api.Mailing
{
    public class MailingSettings
    {
        public long Id { get; set; }
        public string Schedule { get; set; }
        public long NumberOfArticles { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}