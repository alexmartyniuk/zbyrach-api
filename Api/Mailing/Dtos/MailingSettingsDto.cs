using System.ComponentModel.DataAnnotations;

namespace MediumGrabber.Api.Mailing
{
    public class MailingSettingsDto
    {
        [Required]
        public ScheduleType ScheduleType { get; set; }

        [Required]
        public long NumberOfArticles { get; set; }
    }
}