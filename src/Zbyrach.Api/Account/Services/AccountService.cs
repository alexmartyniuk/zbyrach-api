using System;
using System.Threading.Tasks;

namespace Zbyrach.Api.Account
{
    public class AccountService
    {
        private readonly TokenService _tokenService;
        private readonly UsersService _usersService;

        public AccountService(TokenService tokenService, UsersService userService)
        {
            _tokenService = tokenService;
            _usersService = userService;
        }

        public async Task<AccessToken> Login(string googleIdToken)
        {
            var existingToken = await _tokenService.GetTokenByGoogleToken(googleIdToken);
            if (existingToken != null)
            {
                return existingToken;
            }

            var googleToken = await _tokenService.ValidateGoogleToken(googleIdToken);
            if (googleToken == null)
            {
                return null;
            }

            var user = await _usersService.GetUserByEmail(googleToken.email);
            if (user == null)
            {
                user = await _usersService.AddNewUser(new User
                {
                    Email = googleToken.email,
                    Name = $"{googleToken.given_name} {googleToken.family_name}",
                    PictureUrl = googleToken.picture
                });
            }

            var newToken = _tokenService.CreateFromGoogleToken(googleToken, googleIdToken);
            return await _tokenService.SaveToken(user, newToken);
        }

        public async Task Logout()
        {
            var user = await _usersService.GetCurrentUser();
            var token = await _tokenService.GetTokenByUser(user);
            if (token == null)
            {
                throw new Exception("Token was not found for current user.");
            }

            if (!await _tokenService.RemoveToken(token))
            {
                throw new Exception("Token was not removed during logout.");
            }
        }
    }
}