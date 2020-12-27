using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Zbyrach.Api.Account
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/account/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginData)
        {
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
                    PictureUrl = token.User.PictureUrl,
                    IsAdmin = token.User.IsAdmin
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