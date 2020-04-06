using System.ComponentModel.DataAnnotations;

namespace Zbyrach.Api.Mailing
{
    public class MailingSettingsDto
    {
        [Required]
        public ScheduleType ScheduleType { get; set; }

        [Required]
        public long NumberOfArticles { get; set; }
    }
}