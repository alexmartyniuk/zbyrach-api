using System;
using Api.Models;

namespace Api.Services
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

        public AccessToken Login(string idToken)
        {
            var existingToken = _tokenService.GetTokenWithUserByValue(idToken);
            if (existingToken != null)
            {
                return existingToken;
            }

            var googleToken = _tokenService.ValidateGoogleToken(idToken);
            if (googleToken == null)
            {
                return null;
            }

            var user = _usersService.GetUserByEmail(googleToken.email);
            if (user == null)
            {
                user = _usersService.AddNewUser(new User
                {
                    Email = googleToken.email,
                    Name = $"{googleToken.given_name} {googleToken.family_name}",
                    PictureUrl = googleToken.picture
                });
            }

            var newToken = _tokenService.CreateFromGoogleToken(googleToken, idToken);
            return _tokenService.SaveToken(user, newToken);
        }

        public void Logout()
        {
            var user = _usersService.GetCurrentUser();
            var token = _tokenService.GetTokenByUser(user);
            if (token== null)
            {
                throw new Exception("Token was not found for current user.");
            }
            
            if (!_tokenService.RemoveToken(token))
            {
                throw new Exception("Token was not removed during logout.");
            }           
        }
    }
}