using System;
using System.Globalization;

namespace Zbyrach.Api.Mailing
{
    internal class MesageComposerUkrainian : MessageComposerAbstract
    {
        public MesageComposerUkrainian(string webUiBasePath) : base(webUiBasePath)
        {
        }

        protected override string GetFromName()
        {
            return "Збирач";
        }

        protected override string GetTitle()
        {
            return "Cтатті за";
        }

        protected override string GetTemplateFileName()
        {
            return "Articles-uk.html";
        }

        protected override string GetDate(in DateTime date)
        {
            var culture = new CultureInfo("uk-UA");
            return date.ToString("m", culture);
        }

        protected override string GetPeriod(ScheduleType scheduleType)
        {
            switch (scheduleType)
            {
                case ScheduleType.EveryDay:
                    return GetDate(DateTime.UtcNow);
                case ScheduleType.EveryWeek:
                    return "минулий тиждень";
                case ScheduleType.EveryMonth:
                    return "минулий місяць";
                default:
                    throw new ArgumentException(nameof(scheduleType));
            }
        }
    }
}