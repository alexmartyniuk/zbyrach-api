using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Tests
{
    public class ArticleServiceTests : DatabaseTests
    {
        private readonly string USER1_EMAIL = "user1@domain.com";
        private readonly string USER2_EMAIL = "user2@domain.com";
        private readonly string ARTICLE1_URL = "http://domain.com/article1";

        public ArticleServiceTests() : base()
        {
            var user1 = new User
            {
                Email = USER1_EMAIL,
                Name = "User1"
            };
            Context.Users.Add(user1);

            var user2 = new User
            {
                Email = USER2_EMAIL,
                Name = "User2"
            };
            Context.Users.Add(user2);

            var article1 = new Article
            {
                ExternalId = "Article1",
                Url = ARTICLE1_URL
            };
            Context.Articles.Add(article1);


            Context.ArticleUsers.Add(
                new ArticleUser
                {
                    User = user1,
                    Article = article1,
                    Status = ArticleStatus.New
                });

            SaveAndRecreateContext();
        }

        [Fact]
        public async Task SetStatus_ForTwoUsersAndTwoArticles_ShouldSaveStatus()
        {
            var user1 = Context.Users.Single(u => u.Email == USER1_EMAIL);
            var user2 = Context.Users.Single(u => u.Email == USER2_EMAIL);
            var article = Context.Articles.Single(a => a.Url == ARTICLE1_URL);

            var service = new ArticleService(Context);

            await service.SetStatus(
                new List<Article> { article }, 
                new List<User> { user1, user2 }, 
                ArticleStatus.Opened);
            SaveAndRecreateContext();

            var savedArticle = Context
                .Articles
                .Include(a => a.ArticleUsers)
                .ThenInclude(au => au.User)
                .Single(a => a.Url == ARTICLE1_URL);
            
            savedArticle.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().NotBeNull();
            savedArticle.ArticleUsers.Should().HaveCount(2);

            var articleUser1 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER1_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.Opened);

            var articleUser2 = savedArticle.ArticleUsers.Single(au => au.User.Email == USER2_EMAIL);
            articleUser1.Status.Should().Be(ArticleStatus.Opened);
        }
    }
}