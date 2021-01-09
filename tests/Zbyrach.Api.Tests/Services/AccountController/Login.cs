using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Common.Helpers;

namespace Zbyrach.Api.Tests.Services.AccountController
{
    public class Login : BaseControllerTests
    {
        [Fact]
        public async Task ShouldSucceed()
        {
            const string token = "TOKEN";
            DateTime now = DateTime.UtcNow;

            _dateTimeService.Setup(s => s.Now())
                .Returns(now);
            _googleAuthService.Setup(s => s.FindGoogleToken(It.IsAny<string>()))
                .ReturnsAsync(new GoogleToken
                {
                    email = Constants.USER_EMAIL,
                    family_name = Constants.USER_NAME,
                    picture = Constants.USER_PICTURE_URL
                });

            var request = new LoginRequestDto
            {
                Token = token
            };
            var response = await Client.PostJson("/account/login", request);

            response.IsSuccessStatusCode.Should().Be(true);

            var payload = await response.GetBody<LoginResponseDto>();            
            payload.Token.Should().NotBeNullOrEmpty();
            payload.User.Id.Should().BeGreaterThan(0);
            payload.User.Name.Should().Be(Constants.USER_NAME);
            payload.User.Email.Should().Be(Constants.USER_EMAIL);
            payload.User.PictureUrl.Should().Be(Constants.USER_PICTURE_URL);
            payload.User.IsAdmin.Should().Be(false);

            RecreateContext();
            var savedUser = Context
                .Users
                .Include(u => u.AccessTokens)
                .Single();
            savedUser.Id.Should().BeGreaterThan(0);
            savedUser.Email.Should().Be(Constants.USER_EMAIL);
            savedUser.Name.Should().Be(Constants.USER_NAME);
            savedUser.PictureUrl.Should().Be(Constants.USER_PICTURE_URL);
            savedUser.IsAdmin.Should().Be(false);

            var savedToken = savedUser.AccessTokens.Single();
            savedToken.Id.Should().BeGreaterThan(0);
            savedToken.Token.Should().NotBeNullOrEmpty();
            savedToken.ClientIp.Should().Be(Constants.IP_ADDRESS);
            savedToken.ClientUserAgent.Should().Be(Constants.USER_AGENT);
            savedToken.CreatedAt.Should().Be(now);

            _googleAuthService.Verify(s => s.FindGoogleToken(token), Times.Once);
            _googleAuthService.VerifyNoOtherCalls();
        }
    }
}
