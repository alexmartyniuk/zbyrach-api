using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Account.Dto;
using Zbyrach.Api.Common.Helpers;
using Zbyrach.Api.Tests.Common;

namespace Zbyrach.Api.Tests.Account
{
    public class Logout : BaseControllerTests
    {
        [Fact]
        public async Task ShouldRemoveToken_InNormalCase()
        {
            var now = new DateTime(2005, 12, 25);

            _dateTimeService.Setup(s => s.Now())
                .Returns(now);

            Context.Users.Add(new User
            {
                Email = Constants.USER_EMAIL,
                Name = Constants.USER_NAME,
                PictureUrl = Constants.USER_PICTURE_URL,
                AccessTokens = new List<AccessToken>
                {
                    new AccessToken
                    {
                        ClientIp = Constants.IP_ADDRESS,
                        ClientUserAgent = Constants.USER_AGENT,
                        CreatedAt = now.AddDays(-15),
                        Token = Constants.TOKEN
                    }
                },
            });
            SaveAndRecreateContext();
       
            var response = await Client.PostJson("/account/logout", new LogoutRequest());

            response.IsSuccessStatusCode.Should().Be(true);
            
            RecreateContext();
            var savedToken = Context
                .AccessTokens                
                .SingleOrDefault();
            savedToken.Should().BeNull();
        }

        [Fact]
        public async Task ShouldFail_ForNonexistentToken ()
        {
            var now = new DateTime(2005, 12, 25);

            _dateTimeService.Setup(s => s.Now())
                .Returns(now);

            Context.Users.Add(new User
            {
                Email = Constants.USER_EMAIL,
                Name = Constants.USER_NAME,
                PictureUrl = Constants.USER_PICTURE_URL,
                AccessTokens = new List<AccessToken>
                {
                    new AccessToken
                    {
                        ClientIp = Constants.IP_ADDRESS,
                        ClientUserAgent = Constants.USER_AGENT,
                        CreatedAt = now.AddDays(-15),
                        Token = "NOsNEXISTENT_TOKEN"
                    }
                },
            });
            SaveAndRecreateContext();

            var response = await Client.PostJson("/account/logout", new LogoutRequest());

            response.IsSuccessStatusCode.Should().Be(false);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            var problemDetails = await response.GetBody<ProblemDetails>();
            problemDetails.Status.Should().Be(500);
            problemDetails.Title.Should().Be("Internal Server Error");
            problemDetails.Type.Should().Be("https://httpstatuses.com/500");
            problemDetails.Detail.Should().BeNull();
            problemDetails.Instance.Should().BeNull();

            RecreateContext();
            var savedToken = Context
                .AccessTokens
                .SingleOrDefault();
            savedToken.Should().NotBeNull();
        }
    }
}
