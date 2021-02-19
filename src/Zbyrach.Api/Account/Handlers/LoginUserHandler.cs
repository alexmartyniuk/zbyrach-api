using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zbyrach.Api.Account.Exceptions;
using Zbyrach.Api.Common;
using Zbyrach.Api.Migrations;

namespace Zbyrach.Api.Account.Handlers
{
    public class LoginUserHandler : IRequestHandler<LoginRequestDto, LoginResponseDto>
    {
        private readonly ILogger<LoginUserHandler> _logger;
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _accessor;
        private readonly GoogleAuthService _googleAuthService;
        private readonly DateTimeService _dateTimeService;

        public LoginUserHandler(
            ILogger<LoginUserHandler> logger,
            ApplicationContext db,
            IHttpContextAccessor accessor,
            GoogleAuthService googleAuthService,
            DateTimeService dateTimeService)
        {
            _logger = logger;
            _db = db;
            _accessor = accessor;
            _googleAuthService = googleAuthService;
            _dateTimeService = dateTimeService;
        }

        public async Task<LoginResponseDto> Handle(LoginRequestDto request, CancellationToken cancellationToken)
        {
            var googleTokenOption = await _googleAuthService
                .FindGoogleToken(request.Token);
            var googleToken = googleTokenOption
                .IfNone(() => throw new InvalidTokenException("Token is invalid."));

            var userOption = await FindUserByEmail(googleToken.email);
            var user = userOption
                .IfNone(() => AddNewUser(new User
                {
                    Email = googleToken.email,
                    Name = $"{googleToken.given_name} {googleToken.family_name}".Trim(),
                    PictureUrl = googleToken.picture
                })
            );

            var token = await AddNewToken(user);

            await _db.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = token.Token,
                User = new UserDto
                {
                    Id = token.User.Id,
                    Email = token.User.Email,
                    Name = token.User.Name,
                    PictureUrl = token.User.PictureUrl,
                    IsAdmin = token.User.IsAdmin,
                    Language = token.User.Language
                }
            };
        }

        private User AddNewUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new Exception("A new user can't have an empty email.");
            }

            _db.Users.Add(user);

            return user;
        }

        private async Task<Option<User>> FindUserByEmail(string email)
        {
            return await _db.Users
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<AccessToken> AddNewToken(User user)
        {
            var clientIp = GetClientIP();
            var clientUserAgent = GetClientUserAgent();

            var existingTokenOption = await FindToken(user, clientIp, clientUserAgent);
            existingTokenOption.IfSome((token) =>
            {
                RemoveAccessToken(token);
            });

            var token = new AccessToken
            {
                Token = Guid.NewGuid().ToString(),
                ClientIp = clientIp,
                ClientUserAgent = clientUserAgent,
                User = user,
                CreatedAt = _dateTimeService.Now()
            };

            _db.AccessTokens.Add(token);

            _logger.LogInformation($"AccessToken created: ClientIp='{token.ClientIp}' CreateAt='{token.CreatedAt.ToLongDateString()}'");

            return token;
        }

        public async Task<Option<AccessToken>> FindToken(User user, string clientIp, string clientUserAgent)
        {
            return await _db.AccessTokens
                .Where(t => t.ClientIp == clientIp)
                .Where(t => t.ClientUserAgent == clientUserAgent)
                .Where(t => t.UserId == user.Id)
                .SingleOrDefaultAsync();
        }

        public void RemoveAccessToken(AccessToken token)
        {
            _db.AccessTokens.Remove(token);

            _logger.LogInformation($"AccessToken removed: ClientIp='{token.ClientIp}' CreateAt='{token.CreatedAt.ToLongDateString()}'");
        }

        private string GetClientIP()
        {
            return _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        private string GetClientUserAgent()
        {
            return _accessor.HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
        }
    }
}