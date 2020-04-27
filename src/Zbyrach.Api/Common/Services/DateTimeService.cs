using System;

namespace Zbyrach.Api.Common
{
    public class DateTimeService
    {
        public virtual DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
