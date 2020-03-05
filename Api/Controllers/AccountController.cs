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

        public AccountController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost]
        [Route("/account/login")]
        public IActionResult Login([FromBody] LoginDto loginData)
        {
            var existingUser = _usersService.GetUserByEmail(loginData.EmailAddress);

            if (ModelState.IsValid)
            {
                if (existingUser == null)
                {
                    _usersService.AddNewUser(new User
                    {
                        Email = loginData.EmailAddress,
                        Name = $"{loginData.FirstName} {loginData.LastName}",
                        PictureUrl = loginData.PictureUrl
                    });
                }

                return Ok(new { message = "User Login successful" });
            }

            var errors = ModelState.Values.First().Errors;
            return BadRequest(new JsonResult(errors));
        }
    }
}