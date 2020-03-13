using System.ComponentModel.DataAnnotations;

namespace MediumGrabber.Api.Mailing
{
    public class MailingSettingsDto
    {
        [Required]
        public string Schedule { get; set; }

        [Required]
        public long NumberOfArticles { get; set; }
    }
}