using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Tags;

namespace Zbyrach.Api.Tests
{
    public class TagServiceTests : BaseDatabaseTests
    {
        public TagServiceTests() : base()
        {
            var user1 = new User
            {
                Email = "user1@domain.com",
                Name = "User1"
            };
            Context.Users.Add(user1);

            var user2 = new User
            {
                Email = "user2@domain.com",
                Name = "User2"
            };
            Context.Users.Add(user2);

            var tag1 = new Tag
            {
                Name = "Tag1"
            };
            Context.Tags.Add(tag1);

            var tag2 = new Tag
            {
                Name = "Tag2"
            };
            Context.Tags.Add(tag2);

            var tag3 = new Tag
            {
                Name = "Tag3"
            };
            Context.Tags.Add(tag3);

            Context.TagUsers.AddRange(
            new TagUser
            {
                User = user1,
                Tag = tag1
            },
            new TagUser
            {
                User = user1,
                Tag = tag2
            },
            new TagUser
            {
                User = user2,
                Tag = tag2
            },
            new TagUser
            {
                User = user2,
                Tag = tag3
            });

            SaveAndRecreateContext();
        }

        [Fact]
        public async Task GetTagsWithUsers_ForOneUserWithTag_ShouldReturnOneTag()
        {
            var service = new TagService(Context);

            var tags = await service.GetTagsWithUsers();

            tags.Should().NotBeNull();
            tags.Keys.Should().HaveCount(3);

            var tag1Users = tags[tags.Keys.Single(k => k.Name == "Tag1")];
            tag1Users.Should().HaveCount(1);
            tag1Users.Should().Contain(u => u.Name == "User1");

            var tag2Users = tags[tags.Keys.Single(k => k.Name == "Tag2")];
            tag2Users.Should().HaveCount(2);
            tag2Users.Should().Contain(u => u.Name == "User1");
            tag2Users.Should().Contain(u => u.Name == "User2");

            var tag3Users = tags[tags.Keys.Single(k => k.Name == "Tag3")];
            tag3Users.Should().HaveCount(1);
            tag3Users.Should().Contain(u => u.Name == "User2");
        }
    }
}
