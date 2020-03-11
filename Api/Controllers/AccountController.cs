using System.Linq;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [Route("/account/login")]
        public IActionResult Login([FromBody] LoginRequestDto loginData)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.First().Errors;
                return BadRequest(new JsonResult(errors));
            }

            var token = _accountService.Login(loginData.IdToken);
            if (token == null)
            {
                return Unauthorized("Token is invalid");
            }

            var response = new LoginResponseDto
            {
                AuthToken = token.Token,
                User = new UserDto
                {
                    Id = token.User.Id,
                    Email = token.User.Email,
                    Name = token.User.Name,
                    PictureUrl = token.User.PictureUrl
                }
            };
            
            return Ok(response);
        }

        [HttpPost]
        [Route("/account/logout")]
        public IActionResult Logout()
        {
            throw new NotImplemented();
        }
    }
}