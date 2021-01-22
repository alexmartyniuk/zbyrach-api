using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zbyrach.Api.Account
{
    public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string AUTH_TOKEN_PARAM_NAME = "AuthToken";        
        private readonly AccessTokenService _tokenService;

        public AuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            AccessTokenService tokenService)
            : base(options, logger, encoder, clock)
        {
            _tokenService = tokenService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            var authToken = GetAuthToken();
            if (string.IsNullOrEmpty(authToken))
            {
                return AuthenticateResult.Fail("Missing AuthToken Header");
            }

            var tokenWithUser = await _tokenService.FindByToken(authToken);
            if (tokenWithUser == null)
            {
                return AuthenticateResult.Fail("Invalid authentication token");
            }

            var user = tokenWithUser.User;
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Authentication, tokenWithUser.Token),
                new Claim(ClaimTypes.Expired, tokenWithUser.ExpiredAt().ToLongDateString()),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private string GetAuthToken()
        {
            if (Request.Headers.TryGetValue(AUTH_TOKEN_PARAM_NAME, out var headerValue))
            {
                return headerValue.ToString();
            }

            if (Request.Query.TryGetValue(AUTH_TOKEN_PARAM_NAME, out var queryValue))
            {
                return queryValue.ToString();
            }

            return null;
        }
    }
}