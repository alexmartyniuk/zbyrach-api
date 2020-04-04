using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediumGrabber.Api.Account
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/account/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginData)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.First().Errors;
                return BadRequest(new JsonResult(errors));
            }

            var token = await _accountService.Login(loginData.Token);
            if (token == null)
            {
                return Unauthorized("Token is invalid");
            }

            var response = new LoginResponseDto
            {
                Token = token.Token,
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
        public async Task<IActionResult> Logout()
        {
            await _accountService.Logout();
            return Ok();
        }
    }
}