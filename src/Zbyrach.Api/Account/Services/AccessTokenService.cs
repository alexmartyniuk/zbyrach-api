using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Zbyrach.Api.Account
{
    public class AccessTokenService
    {
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<AccessTokenService> _logger;

        public HttpClient _http { get; set; }

        public AccessTokenService(ApplicationContext db, IHttpContextAccessor accessor, ILogger<AccessTokenService> logger)
        {
            _db = db;
            _accessor = accessor;
            _logger = logger;
            _http = new HttpClient();
        }

        public async Task<AccessToken> FindByToken(string token)
        {
            var accessToken = await _db.AccessTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);

            if (accessToken != null && accessToken.ExpiredAt() > DateTime.UtcNow)
            {
                _logger.LogInformation($"AccessToken was found by token {token}");
                return accessToken;
            }

            _logger.LogInformation($"AccessToken was not found by token {token}");
            return null;
        }

        public async Task<AccessToken> FindByUser(User user)
        {
            var accessToken = await _db.AccessTokens                
                .SingleOrDefaultAsync(token => 
                    token.UserId == user.Id &&
                    token.ClientIp == GetClientIP() && 
                    token.ClientUserAgent == GetClientUserAgent());            
                    
            _logger.LogInformation($"AccessToken was found by user: {accessToken?.ClientIp} {accessToken?.CreatedAt.ToLongDateString()}");
            return accessToken;
        }

        private string GetClientIP()
        {
            return _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        private string GetClientUserAgent()
        {
            return _accessor.HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
        }

        public async Task<bool> Remove(AccessToken token)
        {
            var existingToken = await _db.AccessTokens.FindAsync(token.Id);
            if (existingToken == null)
            {
                return false;
            }

            _logger.LogInformation($"AccessToken was removed: {token.ClientIp} {token.CreatedAt.ToLongDateString()}");
            _db.AccessTokens.Remove(existingToken);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<GoogleToken> FindGoogleToken(string idToken)
        {
            try
            {
                var response = await _http.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
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
            var existingToken = await FindByUser(user);
            if (existingToken != null)
            {
                await Remove(existingToken);
            }
           
            var newToken = new AccessToken
            {
                Token = Guid.NewGuid().ToString(),
                ClientIp = GetClientIP(),
                ClientUserAgent = GetClientUserAgent(),
                User = user,
                CreatedAt = DateTime.UtcNow                
            };
                
            _logger.LogInformation($"AccessToken was removed: {newToken.ClientIp} {newToken.CreatedAt.ToLongDateString()}");
            _db.AccessTokens.Add(newToken);
            await _db.SaveChangesAsync();

            return newToken;
        }
    }
}