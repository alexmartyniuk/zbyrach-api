using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Zbyrach.Api.Account;
using Zbyrach.Api.Common.Helpers;
using Zbyrach.Api.Tests.Common;

namespace Zbyrach.Api.Tests.Account
{
    public class Login : BaseControllerTests
    {
        [Fact]
        public async Task ShouldCreateNewUser_AfterFirstLogin()
        {
            const string token = "TOKEN";
            DateTime now = DateTime.UtcNow;

            _dateTimeService.Setup(s => s.Now())
                .Returns(now);
            _googleAuthService.Setup(s => s.FindGoogleToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GoogleTokenInfo
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

            _googleAuthService.Verify(s => s.FindGoogleToken(token, It.IsAny<CancellationToken>()), Times.Once);
            _googleAuthService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotCreateNewUser_AfterSecondLogin()
        {
            const string token = "TOKEN";
            DateTime now = DateTime.UtcNow;

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
                        CreatedAt = new DateTime(2005, 12, 16),
                        Token = Guid.NewGuid().ToString()
                    }
                },
            });
            SaveAndRecreateContext();

            _dateTimeService.Setup(s => s.Now())
                .Returns(now);
            _googleAuthService.Setup(s => s.FindGoogleToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GoogleTokenInfo
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

            _googleAuthService.Verify(s => s.FindGoogleToken(token, It.IsAny<CancellationToken>()), Times.Once);
            _googleAuthService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldCreateNewUser_AfterSecondLoginIfIpDiffers()
        {
            const string token = "TOKEN";
            const string oldTokenIp = "6.6.6.6";
            var oldTokenDate = new DateTime(2005, 12, 16);
            var now = DateTime.UtcNow;

            Context.Users.Add(new User
            {
                Email = Constants.USER_EMAIL,
                Name = Constants.USER_NAME,
                PictureUrl = Constants.USER_PICTURE_URL,
                AccessTokens = new List<AccessToken>
                {
                    new AccessToken
                    {
                        ClientIp = oldTokenIp,
                        ClientUserAgent = Constants.USER_AGENT,
                        CreatedAt = oldTokenDate,
                        Token = Guid.NewGuid().ToString()
                    }
                },
            });
            SaveAndRecreateContext();

            _dateTimeService.Setup(s => s.Now())
                .Returns(now);
            _googleAuthService.Setup(s => s.FindGoogleToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GoogleTokenInfo
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

            savedUser.AccessTokens.Count.Should().Be(2);
            var newToken = savedUser.AccessTokens.Single(at => at.ClientIp == Constants.IP_ADDRESS);
            newToken.Id.Should().BeGreaterThan(0);
            newToken.Token.Should().NotBeNullOrEmpty();            
            newToken.ClientUserAgent.Should().Be(Constants.USER_AGENT);
            newToken.CreatedAt.Should().Be(now);

            var oldToken = savedUser.AccessTokens.Single(at => at.ClientIp == oldTokenIp);
            oldToken.Id.Should().BeGreaterThan(0);
            oldToken.CreatedAt.Should().Be(oldTokenDate);

            _googleAuthService.Verify(s => s.FindGoogleToken(token, It.IsAny<CancellationToken>()), Times.Once);
            _googleAuthService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldFail_ForIncorrectGoogleToken()
        {
            const string token = "TOKEN";            
            _googleAuthService.Setup(s => s.FindGoogleToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GoogleTokenInfo)null);

            var request = new LoginRequestDto
            {
                Token = token
            };
            var response = await Client.PostJson("/account/login", request);

            response.IsSuccessStatusCode.Should().Be(false);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var payload = await response.Content.ReadAsStringAsync();
            payload.Should().NotBeEmpty();

            RecreateContext();
            var savedUser = Context
                .Users                
                .SingleOrDefault();
            savedUser.Should().BeNull();
            
            _googleAuthService.Verify(s => s.FindGoogleToken(token, It.IsAny<CancellationToken>()), Times.Once);
            _googleAuthService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldFail_ForIncorrectRequest()
        {
            var request = new LoginRequestDto
            {
                Token = null
            };
            var response = await Client.PostJson("/account/login", request);

            response.IsSuccessStatusCode.Should().Be(false);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var payload = await response.Content.ReadAsStringAsync();
            payload.Should().Contain("The Token field is required");

            RecreateContext();
            var savedUser = Context
                .Users
                .SingleOrDefault();
            savedUser.Should().BeNull();

            _googleAuthService.VerifyNoOtherCalls();
        }
    }
}
