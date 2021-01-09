using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zbyrach.Api.Migrations;

namespace Zbyrach.Api.Account.Handlers
{
    public class LoginUserHandler : IRequestHandler<LoginRequestDto, LoginResponseDto>
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<AccessTokenService> _logger;
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _accessor;

        public LoginUserHandler(ILogger<AccessTokenService> logger, ApplicationContext db, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _db = db;
            _accessor = accessor;
        }

        public async Task<LoginResponseDto> Handle(LoginRequestDto request, CancellationToken cancellationToken)
        {
            var googleTokenInfo = await FindGoogleToken(request.Token);
            if (googleTokenInfo == null)
            {
                return null;
            }

            var user = await FindUserByEmail(googleTokenInfo.email);
            if (user == null)
            {
                user = await AddNewUser(new User
                {
                    Email = googleTokenInfo.email,
                    Name = $"{googleTokenInfo.given_name} {googleTokenInfo.family_name}",
                    PictureUrl = googleTokenInfo.picture
                });
            }

            var token = await CreateAndSaveNewToken(user);
            if (token == null)
            {
                return null;
            }

            return new LoginResponseDto
            {
                Token = token.Token,
                User = new UserDto
                {
                    Id = token.User.Id,
                    Email = token.User.Email,
                    Name = token.User.Name,
                    PictureUrl = token.User.PictureUrl,
                    IsAdmin = token.User.IsAdmin
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

        private async Task<GoogleToken> FindGoogleToken(string idToken)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                return JsonSerializer.Deserialize<GoogleToken>(response);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Google token could not be validated.");
                return null;
            }
        }

        public async Task<AccessToken> CreateAndSaveNewToken(User user)
        {
            var existingToken = await GetCurrentToken();
            if (existingToken != null)
            {
                await RemoveAccessToken(existingToken);
            }

            var newToken = new AccessToken
            {
                Token = Guid.NewGuid().ToString(),
                ClientIp = GetClientIP(),
                ClientUserAgent = GetClientUserAgent(),
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            _db.AccessTokens.Add(newToken);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"AccessToken created: Id='{newToken.Id}' ClientIp='{newToken.ClientIp}' CreateAt='{newToken.CreatedAt.ToLongDateString()}'");

            return newToken;
        }

        public async Task<AccessToken> GetCurrentToken()
        {
            var token = _accessor.HttpContext.User.FindFirstValue(ClaimTypes.Authentication);
            if (token == null)
            {
                return null;
            }

            return await FindByToken(token);
        }

        public async Task<AccessToken> FindByToken(string token)
        {
            var accessToken = await _db.AccessTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);

            if (accessToken != null && accessToken.ExpiredAt() > DateTime.UtcNow)
            {
                return accessToken;
            }

            return null;
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