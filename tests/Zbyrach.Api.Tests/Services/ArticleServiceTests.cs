using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Tags;

namespace Zbyrach.Api.Tests
{
    public class ArticleServiceTests : DatabaseTests
    {
        private readonly string USER1_EMAIL = "user1@domain.com";
        private readonly string USER2_EMAIL = "user2@domain.com";
        private readonly string TAG_NAME = "Tag";

        private readonly Mock<UsersService> _usersServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<PdfService> _pdfServiceMock;

        public ArticleServiceTests()
        {
            _usersServiceMock = new Mock<UsersService>(MockBehavior.Strict, null, null);
            _configurationMock = new Mock<IConfiguration>();
            _pdfServiceMock = new Mock<PdfService>(MockBehavior.Strict, _configurationMock.Object);
        }

        [Fact]
        public async Task SaveArticle_ForTwoUsersAndTwoArticles_ShouldSucceed()
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

            var originalTag = new Tag
            {
                Name = TAG_NAME
            };
            Context.Tags.Add(originalTag);

            SaveAndRecreateContext();

            _pdfServiceMock.Setup(s => s.QueueArticle(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = new ArticleService(Context, _usersServiceMock.Object, _pdfServiceMock.Object);

            var user1 = Context.Users.Single(u => u.Email == USER1_EMAIL);
            var user2 = Context.Users.Single(u => u.Email == USER2_EMAIL);
            var tag = Context.Tags.Single(t => t.Name == TAG_NAME);
            var article = CreateArticle();

            await service.SaveArticle(
                article,
                new List<User> { user1, user2 },
                tag);
            SaveAndRecreateContext();

            var savedArticle = Context
                .Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.ArticleUsers)
                .ThenInclude(au => au.User)
                .Single(a => a.Title == article.Title);

            savedArticle.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().HaveCount(2);

            var articleUser1 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER1_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.New);

            var articleUser2 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER2_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.New);

            savedArticle.ArticleTags.Should().NotBeNull();
            savedArticle.ArticleTags.Should().HaveCount(1);
            var articleTag = savedArticle.ArticleTags.Single(at => at.Tag.Name == TAG_NAME);

            _pdfServiceMock.Verify(s => s.QueueArticle(article.Url), Times.Once);
            _pdfServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SaveArticle_ForExistsArticleWithNewUsers_ShouldSucceed()
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

            var originalTag = new Tag
            {
                Name = TAG_NAME
            };
            Context.Tags.Add(originalTag);

            var originalArticle = CreateArticle();
            Context.Articles.Add(originalArticle);

            var originalArticleTag = new ArticleTag
            {
                Article = originalArticle,
                Tag = originalTag
            };
            Context.ArticleTags.Add(originalArticleTag);

            var originalArticleUser = new ArticleUser
            {
                Article = originalArticle,
                User = originalUser1,
                Status = ArticleStatus.New
            };
            Context.ArticleUsers.Add(originalArticleUser);

            SaveAndRecreateContext();

            _pdfServiceMock.Setup(s => s.QueueArticle(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = new ArticleService(Context, _usersServiceMock.Object, _pdfServiceMock.Object);
            
            var user2 = Context.Users.Single(u => u.Email == USER2_EMAIL);
            var tag = Context.Tags.Single(t => t.Name == TAG_NAME);
            var article = new Article
            { 
                Title = originalArticle.Title,
                Url = originalArticle.Url
            };

            await service.SaveArticle(
                article,
                new List<User> { user2 },
                tag);
            SaveAndRecreateContext();

            var articles = Context.Articles.ToList();
            
            var savedArticle = Context
                .Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.ArticleUsers)
                .ThenInclude(au => au.User)
                .Single(a => a.Title == article.Title);

            savedArticle.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().HaveCount(2);

            var articleUser1 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER1_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.New);

            var articleUser2 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER2_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.New);

            savedArticle.ArticleTags.Should().NotBeNull();
            savedArticle.ArticleTags.Should().HaveCount(1);
            var articleTag = savedArticle.ArticleTags.Single(at => at.Tag.Name == TAG_NAME);

            _pdfServiceMock.Verify(s => s.QueueArticle(article.Url), Times.Never);
            _pdfServiceMock.VerifyNoOtherCalls();
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

            var service = new ArticleService(Context, _usersServiceMock.Object, _pdfServiceMock.Object);

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

            var service = new ArticleService(Context, _usersServiceMock.Object, _pdfServiceMock.Object);

            var result = await service.GetLastMailSentDateByUsers();

            result.Should().NotBeNull();
            result.Keys.Should().HaveCount(2);
            result.Values.Should().HaveCount(2);

            var dateTime1 = result[originalUser1];
            dateTime1.Should().Be(originalSentDate2);

            var dateTime2 = result[originalUser2];
            dateTime2.Should().Be(originalSentDate6);
        }

        [Fact]
        public async Task GetForSending_ForListOfArticles_ShouldReturnArticlesInRightOrder()
        {
            var user = new User
            {
                Email = USER1_EMAIL,
                Name = "User1"
            };
            Context.Users.Add(user);

            var article1 = CreateArticle();
            article1.PublicatedAt = new DateTime(2020, 09, 12);
            article1.CommentsCount = 10;
            article1.LikesCount = 100;

            var article2 = CreateArticle();
            article2.PublicatedAt = new DateTime(2020, 09, 13);
            article2.CommentsCount = 30;
            article2.LikesCount = 300;

            var article3 = CreateArticle();
            article3.PublicatedAt = new DateTime(2020, 09, 14);
            article3.CommentsCount = 20;
            article3.LikesCount = 200;

            var articles = new List<Article> { article1, article2, article3 };
            Context.Articles.AddRange(articles);

            Context.ArticleUsers.AddRange(
                articles.Select(a =>
                    new ArticleUser
                    {
                        User = user,
                        Article = a,
                        Status = ArticleStatus.New,
                    }));
            SaveAndRecreateContext();

            var service = new ArticleService(Context, _usersServiceMock.Object, _pdfServiceMock.Object);

            var result = await service.GetForSending(user, 10);

            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            var art1 = result.ElementAt(0);
            art1.Id.Should().Be(article2.Id);

            var art2 = result.ElementAt(1);
            art2.Id.Should().Be(article3.Id);

            var art3 = result.ElementAt(2);
            art3.Id.Should().Be(article1.Id);
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