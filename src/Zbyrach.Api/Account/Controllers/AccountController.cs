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
            _logger.LogInformation("Started login");
            var token = await _accountService.Login(loginData.Token);
            if (token == null)
            {
                _logger.LogInformation("Google Id Token is invalid");
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

            _logger.LogInformation("Login OK");
            return Ok(response);
        }

        [HttpPost]
        [Route("/account/logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Started logout");
            await _accountService.Logout();
            _logger.LogInformation("Logout OK");
            return Ok();
        }
    }
}