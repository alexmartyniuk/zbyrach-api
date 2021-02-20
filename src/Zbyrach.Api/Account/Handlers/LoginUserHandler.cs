using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var googleTokenInfo = await _googleAuthService
                .FindGoogleToken(request.Token ,cancellationToken);
            if (googleTokenInfo == null)
            { 
                throw new InvalidTokenException("Token is invalid.");
            }

            var user = await FindUserByEmail(googleTokenInfo.email);
            if (user == null)
            {
                user = AddNewUser(googleTokenInfo);
            }                

            var token = await AddNewToken(user);

            await _db.SaveChangesAsync(cancellationToken);

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

        private User AddNewUser(GoogleTokenInfo googleTokenInfo)
        {
            if (string.IsNullOrWhiteSpace(googleTokenInfo.email))
            {
                throw new Exception("A new user can't have an empty email.");
            }

            var user = new User
            {
                Email = googleTokenInfo.email,
                Name = $"{googleTokenInfo.given_name} {googleTokenInfo.family_name}".Trim(),
                PictureUrl = googleTokenInfo.picture
            };

            _db.Users.Add(user);

            return user;
        }

        private async Task<User?> FindUserByEmail(string email)
        {
            return await _db.Users
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<AccessToken> AddNewToken(User user)
        {
            var clientIp = GetClientIP();
            var clientUserAgent = GetClientUserAgent();

            var existingToken = await FindToken(user, clientIp, clientUserAgent);
            if (existingToken != null)
            {
                RemoveAccessToken(existingToken);
            }            

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

        public async Task<AccessToken> FindToken(User user, string clientIp, string clientUserAgent)
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
            return _accessor
                .HttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString() ?? string.Empty;
        }

        private string GetClientUserAgent()
        {
            return _accessor
                .HttpContext?
                .Request?
                .Headers["User-Agent"]
                .FirstOrDefault() ?? string.Empty;
        }
    }
}