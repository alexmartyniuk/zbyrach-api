using System;
using System.Globalization;

namespace Zbyrach.Api.Mailing
{
    internal class MesageComposerEnglish : MessageComposerAbstract
    {
        public MesageComposerEnglish(string webUiBasePath) : base(webUiBasePath)
        {
        }

        protected override string GetFromName()
        {
            return "Zbyrach";
        }

        protected override string GetTitle()
        {
            return "Articles for the";
        }

        protected override string GetTemplateFileName()
        {
            return "Articles-en.html";
        }

        protected override string GetDate(in DateTime date)
        {
            var culture = new CultureInfo("en-GB");
            return date.ToString("m", culture);
        }

        protected override string GetPeriod(ScheduleType scheduleType)
        {
            switch (scheduleType)
            {
                case ScheduleType.EveryDay:
                    return GetDate(DateTime.UtcNow);
                case ScheduleType.EveryWeek:
                    return "last week";
                case ScheduleType.EveryMonth:
                    return "last month";
                default:
                    throw new ArgumentException(nameof(scheduleType));
            }
        }
    }
}