using System;
using FluentAssertions;
using Moq;
using Xunit;
using Zbyrach.Api.Common;
using Zbyrach.Api.Mailing;

namespace Zbyrach.Api.Tests.Mailing
{
    public class CronServiceTests
    {
        private readonly Mock<DateTimeService> _dateTimeServiceMock = new Mock<DateTimeService>(MockBehavior.Strict);

        [Theory]
        [InlineData("2020-08-07 09:01", "2020-08-07 09:00", "0 9 ? * *", false)] // every day
        [InlineData("2020-08-07 09:01", "2020-08-07 08:30", "0 9 ? * *", false)] // every day
        [InlineData("2020-08-07 09:01", "2020-08-06 09:00", "0 9 ? * *", true)]  // every day
        [InlineData("2020-08-07 09:01", "2020-08-06 11:59", "0 9 ? * *", true)]  // every day
        [InlineData("2020-08-07 09:00", "2020-08-06 11:59", "0 9 ? * *", false)]  // every day
        [InlineData("2020-08-07 09:01", "2020-09-07 00:01", "0 9 ? * *", false)]  // every day
        public void HasTimeCome_ShouldReturnExpectedResult(string now, string sendDate, string schedule, bool expectedResult)
        {
            var dateTime = DateTime.SpecifyKind(DateTime.ParseExact(sendDate, "yyyy-MM-dd hh:mm", null), DateTimeKind.Utc);
            var nowTime = DateTime.SpecifyKind(DateTime.ParseExact(now, "yyyy-MM-dd hh:mm", null), DateTimeKind.Utc);

            _dateTimeServiceMock
                .Setup(s => s.Now())
                .Returns(nowTime);
            var service = new CronService(_dateTimeServiceMock.Object);                      

            var result = service.HasTimeCome(dateTime, TimeSpan.FromMinutes(60), schedule);
            result.Should().Be(expectedResult);
        }
    }
}