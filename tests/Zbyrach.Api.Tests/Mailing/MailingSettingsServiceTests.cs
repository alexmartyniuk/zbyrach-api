using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Tests.Common;

namespace Zbyrach.Api.Tests.Mailing
{
    public class MailingSettingsServiceTests : BaseServiceTests
    {
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
            _cronService = new CronService(_dateTimeService.Object);

            var updatedAt = new DateTime(2020, 04, 27);
            foreach (var item in _data)
            {
                var user = CreateUser(item.Item1);
                var mailingSettings = new MailingSettings
                {
                    User = user,
                    Schedule = _cronService.ScheduleToExpression(item.Item2),
                    UpdatedAt = updatedAt
                };
                Context.Add(mailingSettings);

                var article = new Article
                {
                    Url = "url",
                    Title = "Title"
                };
                article.ArticleUsers.Add(new ArticleUser
                {
                    User = user,
                    Status = ArticleStatus.Sent,
                    SentAt = ParseDateTime(item.Item3)
                });

                Context.Add(article);
            }

            SaveAndRecreateContext();
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

            _dateTimeService.Setup(s => s.Now())
                .Returns(datetime);

            var service = GetSut<MailingSettingsService>();
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
