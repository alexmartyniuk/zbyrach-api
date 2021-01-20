using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zbyrach.Api.Common;
using Zbyrach.Api.Migrations;

namespace Zbyrach.Api.Account.Handlers
{
    public class LoginUserHandler : IRequestHandler<LoginRequestDto, LoginResponseDto>
    {        
        private readonly ILogger<AccessTokenService> _logger;
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _accessor;
        private readonly GoogleAuthService _googleAuthService;
        private readonly DateTimeService _dateTimeService;

        public LoginUserHandler(
            ILogger<AccessTokenService> logger, 
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
            var googleTokenInfo = await _googleAuthService.FindGoogleToken(request.Token);
            if (googleTokenInfo == null)
            {
                _logger.LogError("Google token could not be validated.");
                return null;
            }

            var user = await FindUserByEmail(googleTokenInfo.email);
            if (user == null)
            {
                user = await AddNewUser(new User
                {
                    Email = googleTokenInfo.email,
                    Name = $"{googleTokenInfo.given_name} {googleTokenInfo.family_name}".Trim(),
                    PictureUrl = googleTokenInfo.picture
                });
            }

            var token = await CreateAndSaveNewToken(user);
            
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

        private async Task<User> AddNewUser(User user)
        {
            if (user.Id != default)
            {
                throw new Exception("A new user should not have an Id.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new Exception("A new user should have not empty email.");
            }

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return user;
        }

        private Task<User> FindUserByEmail(string email)
        {
            return _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        }       

        public async Task<AccessToken> CreateAndSaveNewToken(User user)
        {
            var clientIp = GetClientIP();
            var clientUserAgent = GetClientUserAgent();
            
            var existingToken = await FindToken(user, clientIp, clientUserAgent);
            if (existingToken != null)
            {
                await RemoveAccessToken(existingToken);
            }

            var newToken = new AccessToken
            {
                Token = Guid.NewGuid().ToString(),
                ClientIp = clientIp,
                ClientUserAgent = clientUserAgent,
                User = user,
                CreatedAt = _dateTimeService.Now()
            };

            _db.AccessTokens.Add(newToken);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"AccessToken created: Id='{newToken.Id}' ClientIp='{newToken.ClientIp}' CreateAt='{newToken.CreatedAt.ToLongDateString()}'");

            return newToken;
        }

        public Task<AccessToken> FindToken(User user, string clientIp, string clientUserAgent)
        {
            return _db.AccessTokens
                .Where(t => t.ClientIp == clientIp)
                .Where(t => t.ClientUserAgent == clientUserAgent)
                .Where(t => t.UserId == user.Id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> RemoveAccessToken(AccessToken token)
        {
            var existingToken = await _db.AccessTokens.FindAsync(token.Id);
            if (existingToken == null)
            {
                return false;
            }

            _db.AccessTokens.Remove(existingToken);
            var entriesWritten = await _db.SaveChangesAsync();

            _logger.LogInformation($"AccessToken removed: Id='{token.Id}' ClientIp='{token.ClientIp}' CreateAt='{token.CreatedAt.ToLongDateString()}'");

            return entriesWritten > 0;
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