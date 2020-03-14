using System;
using System.Collections.Generic;

namespace MediumGrabber.Api.Mailing
{
    public class CronService
    {
        private readonly Dictionary<string, ScheduleType> _map = new Dictionary<string, ScheduleType>
        {
            {"0 0 9 ? * * *", ScheduleType.EveryDay},
            {"0 0 9 ? * FRI *", ScheduleType.EveryWeek},
            {"0 0 9 L * ?", ScheduleType.EveryMonth}
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

    }
}