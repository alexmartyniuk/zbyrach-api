using System.ComponentModel.DataAnnotations;

namespace Zbyrach.Api.Mailing
{
    public class SettingsSummaryDto
    {
        public ScheduleType ScheduleType { get; set; }

        public long NumberOfTags { get; set; }
    }
}