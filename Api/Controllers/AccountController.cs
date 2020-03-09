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
            if (ModelState.IsValid)
            {
                var errors = ModelState.Values.First().Errors;
                return BadRequest(new JsonResult(errors));
            }

            var loginedUser = _tokenService.GetUserByToken(loginData.AuthToken);
            if (loginedUser != null)
            {
                // User was found, return User data
                return Ok(loginedUser);
            }                       

            var user = _usersService.GetUserByEmail(loginData.EmailAddress);
            if (user == null)
            {
                user = new User
                {
                    Email = loginData.EmailAddress,
                    Name = $"{loginData.FirstName} {loginData.LastName}",
                    PictureUrl = loginData.PictureUrl
                };                
            }

            var isTokenValid = _tokenService.CheckLoginData(loginData, user);
            if (!isTokenValid)
            {
                return Unauthorized("Token is invalid");
            }

            if (user.Id == 0)
            {
                _usersService.AddNewUser(user);
            }    
            
            var token = _tokenService.SaveToken(loginData, user);
            return Ok(token);
        }
    }
}