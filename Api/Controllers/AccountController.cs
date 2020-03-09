using System.Linq;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsersService _usersService;
        private readonly TokenService _tokenService;

        public AccountController(UsersService usersService, TokenService tokenService)
        {
            _usersService = usersService;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("/account/login")]
        public IActionResult Login([FromBody] LoginDto loginData)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.First().Errors;
                return BadRequest(new JsonResult(errors));
            }

            var existingToken = _tokenService.GetTokenWithUser(loginData.AuthToken);
            if (existingToken != null)
            {
                // Valid token was found, return User data
                return Ok(existingToken.User);
            }                       

            var validToken = _tokenService.ValidateToken(loginData.AuthToken);
            if (validToken == null)
            {
                return Unauthorized("Token is invalid");
            }

            var user = _usersService.GetUserByEmail(loginData.EmailAddress);
            if (user == null)
            {
                user = _usersService.AddNewUser(new User
                {
                    Email = loginData.EmailAddress,
                    Name = $"{loginData.FirstName} {loginData.LastName}",
                    PictureUrl = loginData.PictureUrl
                });               
            }

            var savedToken = _tokenService.SaveToken(user, validToken);
            return Ok(savedToken);
        }
    }
}