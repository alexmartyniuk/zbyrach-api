using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Zbyrach.Api.Tests.Services
{
    public class AuthenticatedTestRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticatedTestRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // fake authenticated the user
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, Constants.USER_NAME)
            }, "TokenAuthentication");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            context.User = claimsPrincipal;

            if (IPAddress.TryParse(Constants.IP_ADDRESS, out var ip))
            {
                context.Connection.RemoteIpAddress = ip;
            }

            context.Request.Headers.Add("User-Agent", new StringValues(Constants.USER_AGENT));

            await _next(context);
        }
    }
}
