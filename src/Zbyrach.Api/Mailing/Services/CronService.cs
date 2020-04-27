using System;
using System.Collections.Generic;
using Cronos;
using Zbyrach.Api.Common;

namespace Zbyrach.Api.Mailing
{
    public class CronService
    {
        private readonly DateTimeService _dateTimeService;

        public CronService(DateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
        }

        private readonly Dictionary<string, ScheduleType> _map = new Dictionary<string, ScheduleType>
        {
            {"0 9 ? * *", ScheduleType.EveryDay}, // every day at 9:00
            {"0 9 ? * FRI", ScheduleType.EveryWeek}, // every Friday at 9:00
            {"0 9 L * ?", ScheduleType.EveryMonth}, // every last day of the month at 9:00
            {"", ScheduleType.Never}
        };

        public ScheduleType ExpressionToSchedule(string expression)
        {
            if (_map.TryGetValue(expression, out ScheduleType result))
            {
                return result;
            }

            return ScheduleType.Undefined;
        }

        public string ScheduleToExpression(ScheduleType schedule)
        {
            foreach (var pair in _map)
            {
                if (pair.Value == schedule)
                {
                    return pair.Key;
                }
            }

            return string.Empty;
        }

        public bool HasTimeCome(DateTime dateFrom, TimeSpan schedulePeriod, string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            var nextUtc = CronExpression.Parse(expression).GetNextOccurrence(dateFrom);
            if (!nextUtc.HasValue)
            {
                return false;
            }

            return (nextUtc < _dateTimeService.Now() + schedulePeriod);
        }
    }
}