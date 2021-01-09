using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

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
                return accessToken;
            }

            return null;
        }

        public async Task<bool> Remove(AccessToken token)
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

        public async Task<AccessToken> GetCurrentToken()
        {
            var token = _accessor.HttpContext.User.FindFirstValue(ClaimTypes.Authentication);
            if (token == null)
            {
                return null;
            }

            return await FindByToken(token);
        }
    }
}