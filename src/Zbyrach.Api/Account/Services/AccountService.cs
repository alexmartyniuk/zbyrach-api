using System;
using System.Threading.Tasks;

namespace Zbyrach.Api.Account
{
    public class AccountService
    {
        private readonly AccessTokenService _tokenService;

        public AccountService(AccessTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task Logout()
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