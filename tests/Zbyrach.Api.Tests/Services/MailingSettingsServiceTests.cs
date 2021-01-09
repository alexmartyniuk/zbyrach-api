using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Common;
using Zbyrach.Api.Mailing;

namespace Zbyrach.Api.Tests.Services
{
    public class MailingSettingsServiceTests : BaseDatabaseTests
    {
        private readonly Mock<ArticleService> _articleServiceMock;
        private readonly Mock<DateTimeService> _dateTimeServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<PdfService> _pdfServiceMock;

        private readonly CronService _cronService;

        private readonly List<(string, ScheduleType, string)> _data = new List<(string, ScheduleType, string)>
        {
            ("User1", ScheduleType.EveryDay, "2020-04-27 09:00"),
            ("User2", ScheduleType.EveryDay, "2020-04-28 09:00"),
            ("User3", ScheduleType.EveryDay, "0001-01-01 00:00"),
            ("User4", ScheduleType.EveryWeek, "2020-04-24 09:00"),
            ("User5", ScheduleType.EveryWeek, "2020-05-01 09:00"),
            ("User6", ScheduleType.EveryWeek, "0001-01-01 00:00"),
            ("User7", ScheduleType.EveryMonth, "2020-03-31 09:00"),
            ("User8", ScheduleType.EveryMonth, "2020-04-30 09:00"),
            ("User9", ScheduleType.EveryMonth, "0001-01-01 00:00"),
            ("User10", ScheduleType.Never, "2020-04-27 09:00"),
            ("User11", ScheduleType.Never, "0001-01-01 00:00")
        };

        public MailingSettingsServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _pdfServiceMock = new Mock<PdfService>(MockBehavior.Strict, _configurationMock.Object);
            _articleServiceMock = new Mock<ArticleService>(MockBehavior.Strict, null, null, _pdfServiceMock.Object);
            _dateTimeServiceMock = new Mock<DateTimeService>(MockBehavior.Strict);
            _cronService = new CronService(_dateTimeServiceMock.Object);

            var updatedAt = new DateTime(2020, 04, 27);
            foreach (var item in _data)
            {
                Context.Add(new MailingSettings
                {
                    User = CreateUser(item.Item1),
                    Schedule = _cronService.ScheduleToExpression(item.Item2),
                    UpdatedAt = updatedAt
                });
            }

            SaveAndRecreateContext();

            var savedUsers = Context.Users.ToList();
            var lastSentDays = _data.ToDictionary(d => savedUsers.Single(u => u.Name == d.Item1),
                d => ParseDateTime(d.Item3));
            _articleServiceMock
                .Setup(x => x.GetLastMailSentDateByUsers())
                .ReturnsAsync(lastSentDays);
        }

        [Theory]
        [InlineData("2020-04-27 09:01", "User3,User6,User9")]                         // Monday
        [InlineData("2020-04-28 09:01", "User1,User3,User6,User9")]                   // Thuesday
        [InlineData("2020-04-29 09:01", "User1,User2,User3,User6,User9")]             // Wednesday
        [InlineData("2020-04-30 09:01", "User1,User2,User3,User6,User7,User9")]       // Thursday
        [InlineData("2020-05-01 09:01", "User1,User2,User3,User4,User6,User7,User9")] // Friday
        [InlineData("2020-05-02 09:01", "User1,User2,User3,User4,User6,User7,User9")] // Saturday
        [InlineData("2020-05-03 09:01", "User1,User2,User3,User4,User6,User7,User9")] // Sunday
        public async Task GetBySchedule_ForParticularDay_ShouldReturnCorrectMailSettings(string now, string users)
        {
            var datetime = ParseDateTime(now);
            var userNames = users.Split(',');

            _dateTimeServiceMock.Setup(s => s.Now())
                .Returns(datetime);

            var service = new MailingSettingsService(Context, _cronService, _articleServiceMock.Object);
            var settings = await service.GetBySchedule(TimeSpan.FromMinutes(30));

            settings.Should().HaveCount(userNames.Length);
            foreach (var userName in userNames)
            {
                settings.Count(s => s.User.Name == userName).Should().Be(1);
            }
        }

        private User CreateUser(string name)
        {
            return new User
            {
                Name = name,
                Email = $"{name}@domain.com",
            };
        }

        private DateTime ParseDateTime(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return DateTime.ParseExact(value, "yyyy-MM-dd hh:mm", null);
        }

    }
}
