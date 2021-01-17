using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zbyrach.Api.Account.Dto;

namespace Zbyrach.Api.Account.Handlers
{
    public class LogoutUserHandler : AsyncRequestHandler<LogoutRequest>
    {
        private readonly AccessTokenService _tokenService;

        public LogoutUserHandler(AccessTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            var token = await _tokenService.GetCurrentToken();
            if (token == null)
            {
                throw new Exception("Token was not found for current user.");
            }

            if (!await _tokenService.Remove(token))
            {
                throw new Exception("Token was not removed during logout.");
            }
        }
    }
}
