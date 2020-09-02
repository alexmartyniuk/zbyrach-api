using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Tests
{
    public class ArticleServiceTests : DatabaseTests
    {
        private readonly string USER1_EMAIL = "user1@domain.com";
        private readonly string USER2_EMAIL = "user2@domain.com";

        private readonly Mock<UsersService> _usersServiceMock = new Mock<UsersService>(MockBehavior.Strict, null, null);

        [Fact]
        public async Task SetStatus_ForTwoUsersAndTwoArticles_ShouldSaveStatus()
        {
            var originalUser1 = new User
            {
                Email = USER1_EMAIL,
                Name = "User1"
            };
            Context.Users.Add(originalUser1);

            var originalUser2 = new User
            {
                Email = USER2_EMAIL,
                Name = "User2"
            };
            Context.Users.Add(originalUser2);

            var originalArticle = CreateArticle();
            Context.Articles.Add(originalArticle);

            Context.ArticleUsers.Add(
                new ArticleUser
                {
                    User = originalUser1,
                    Article = originalArticle,
                    Status = ArticleStatus.New
                });

            SaveAndRecreateContext();

            var user1 = Context.Users.Single(u => u.Email == USER1_EMAIL);
            var user2 = Context.Users.Single(u => u.Email == USER2_EMAIL);
            var article = Context.Articles.Single(a => a.Title == originalArticle.Title);

            var service = new ArticleService(Context, _usersServiceMock.Object);

            await service.SetStatus(
                article, 
                new List<User> { user1, user2 }, 
                ArticleStatus.Read);
            SaveAndRecreateContext();

            var savedArticle = Context
                .Articles
                .Include(a => a.ArticleUsers)
                .ThenInclude(au => au.User)
                .Single(a => a.Title == originalArticle.Title);
            
            savedArticle.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().HaveCount(2);

            var articleUser1 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER1_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.Read);

            var articleUser2 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER2_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.Read);
        }

        [Fact]
        public async Task GetLastMailSentDateByUsers_ForOneUser_ShouldReturnResult()
        {
            var originalSentDate1 = new DateTime(2020, 02, 05);
            var originalSentDate2 = new DateTime(2020, 03, 01);
            var originalUser = new User
            {
                Email = USER1_EMAIL,
                Name = "User1"
            };
            Context.Users.Add(originalUser);

            Context.ArticleUsers.AddRange(
                new ArticleUser
                {
                    User = originalUser,
                    Article = CreateArticle(),
                    Status = ArticleStatus.Sent,
                    SentAt = originalSentDate1,
                },
                new ArticleUser
                {
                    User = originalUser,
                    Article = CreateArticle(),
                    Status = ArticleStatus.Sent,
                    SentAt = originalSentDate2,
                });

            SaveAndRecreateContext();
            
            var service = new ArticleService(Context, _usersServiceMock.Object);

            var result = await service.GetLastMailSentDateByUsers();

            result.Should().NotBeNull();
            result.Keys.Should().HaveCount(1);
            result.Values.Should().HaveCount(1);

            var dateTime = result[originalUser];
            dateTime.Should().Be(originalSentDate2);
        }

        [Fact]
        public async Task GetLastMailSentDateByUsers_ForTwoUserWithOtherStatuses_ShouldReturnResult()
        {
            var originalSentDate1 = new DateTime(2020, 01, 15);
            var originalSentDate2 = new DateTime(2020, 02, 14);
            var originalSentDate3 = new DateTime(2020, 03, 13);
            var originalSentDate4 = new DateTime(2020, 04, 12);
            var originalSentDate5 = new DateTime(2020, 05, 11);
            var originalSentDate6 = new DateTime(2020, 06, 10);

            var originalUser1 = new User
            {
                Email = USER1_EMAIL,
                Name = "User1"
            };
            Context.Users.Add(originalUser1);

            var originalUser2 = new User
            {
                Email = USER2_EMAIL,
                Name = "User2"
            };
            Context.Users.Add(originalUser1);

            Context.ArticleUsers.AddRange(               
                new ArticleUser
                {
                    // This is a reading with max sent date for user 1
                    User = originalUser1,
                    Article = CreateArticle(),
                    Status = ArticleStatus.Sent,
                    SentAt = originalSentDate2,
                },
                 new ArticleUser
                 {
                     User = originalUser1,
                     Article = CreateArticle(),
                     Status = ArticleStatus.Sent,
                     SentAt = originalSentDate1,
                 },
                 new ArticleUser
                 {                     
                     User = originalUser1,
                     Article = CreateArticle(),
                     Status = ArticleStatus.New,
                     SentAt = originalSentDate3,
                 },
                 new ArticleUser
                 {
                     User = originalUser2,
                     Article = CreateArticle(),
                     Status = ArticleStatus.Sent,
                     SentAt = originalSentDate4,
                 },
                 new ArticleUser
                 {
                     // This is a reading with max sent date for user 2
                     User = originalUser2,
                     Article = CreateArticle(),
                     Status = ArticleStatus.Sent,
                     SentAt = originalSentDate6,
                 },
                 new ArticleUser
                 {
                     User = originalUser2,
                     Article = CreateArticle(),
                     Status = ArticleStatus.Sent,
                     SentAt = originalSentDate5,
                 });

            SaveAndRecreateContext();

            var service = new ArticleService(Context, _usersServiceMock.Object);

            var result = await service.GetLastMailSentDateByUsers();

            result.Should().NotBeNull();
            result.Keys.Should().HaveCount(2);
            result.Values.Should().HaveCount(2);

            var dateTime1 = result[originalUser1];
            dateTime1.Should().Be(originalSentDate2);

            var dateTime2 = result[originalUser2];
            dateTime2.Should().Be(originalSentDate6);
        }

        private Article CreateArticle()
        {
            return new Article
            {
                Title = Guid.NewGuid().ToString(),
                Url = "http://domain.com/article"
            };
        }
    }
}