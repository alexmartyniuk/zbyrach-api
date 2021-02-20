using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Zbyrach.Api.Account.Dto;
using Zbyrach.Api.Migrations;

namespace Zbyrach.Api.Account.Handlers
{
    public class LogoutUserHandler : AsyncRequestHandler<LogoutRequest>
    {
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<LogoutUserHandler> _logger;

        public LogoutUserHandler(ApplicationContext db, IHttpContextAccessor httpContext, ILogger<LogoutUserHandler> logger)
        {
            _db = db;
            _httpContext = httpContext;
            _logger = logger;
        }

        protected override async Task Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            var accessToken = await GetCurrentToken();
            if (accessToken == null)
            {
                throw new Exception("Access token was not found for current user.");
            }

            if (!await Remove(accessToken))
            {
                throw new Exception("Access token was not removed during logout.");
            }
        }

        private async Task<AccessToken?> GetCurrentToken()
        {
            var tokenId = _httpContext
                .HttpContext?
                .User
                .FindFirstValue(ClaimTypes.Authentication);
            if (tokenId == null)
            {
                return null;
            }

            return await _db.AccessTokens
                .SingleOrDefaultAsync(t => t.Token == tokenId);
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
    }
}
