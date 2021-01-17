using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Zbyrach.Api.Common;

namespace Zbyrach.Api.Account
{
    public class AccessTokenService
    {
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _accessor;
        private readonly DateTimeService _dateTimeService;
        private readonly ILogger<AccessTokenService> _logger;

        public AccessTokenService(ApplicationContext db, IHttpContextAccessor accessor, DateTimeService dateTimeService, ILogger<AccessTokenService> logger)
        {
            _db = db;
            _accessor = accessor;
            _dateTimeService = dateTimeService;
            _logger = logger;
        }

        public async Task<AccessToken> FindByToken(string token)
        {
            var accessToken = await _db.AccessTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);

            if (accessToken != null && accessToken.ExpiredAt() > _dateTimeService.Now())
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