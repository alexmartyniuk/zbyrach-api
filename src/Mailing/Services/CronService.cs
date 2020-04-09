using System;
using System.Collections.Generic;
using Cronos;

namespace Zbyrach.Api.Mailing
{
    public class CronService
    {
        private readonly Dictionary<string, ScheduleType> _map = new Dictionary<string, ScheduleType>
        {
            {"0 9 ? * *", ScheduleType.EveryDay},
            {"0 9 ? * FRI", ScheduleType.EveryWeek},
            {"0 9 L * ?", ScheduleType.EveryMonth},
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
            var nextUtc = CronExpression.Parse(expression).GetNextOccurrence(dateFrom);
            if (!nextUtc.HasValue)
            {
                return false;
            }

            return (nextUtc < DateTime.UtcNow + schedulePeriod);
        }
    }
}