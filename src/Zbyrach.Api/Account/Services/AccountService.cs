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

        public async Task<AccessToken> Login(string googleToken)
        {                       
            var googleTokenInfo = await _tokenService.ValidateGoogleToken(googleToken);
            if (googleTokenInfo == null)
            {
                return null;
            }

            var user = await _usersService.FindUserByEmail(googleTokenInfo.email);
            if (user == null)
            {
                user = await _usersService.AddNewUser(new User
                {
                    Email = googleTokenInfo.email,
                    Name = $"{googleTokenInfo.given_name} {googleTokenInfo.family_name}",
                    PictureUrl = googleTokenInfo.picture
                });
            }

            return await _tokenService.CreateAndSaveNewToken(user);
        }

        public async Task Logout()
        {
            var user = await _usersService.GetCurrentUser();
            var token = await _tokenService.FindTokenForUser(user);
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