using System.Threading.Tasks;
using MediatR;
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
        private readonly IMediator _mediator; 

        public AccountController(AccountService accountService, ILogger<AccountController> logger, IMediator mediator)
        {
            _accountService = accountService;
            _logger = logger;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/account/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginData)
        {
            var response = await _mediator.Send(loginData);
            if (response == null)
            {
                return Unauthorized("Token is invalid");
            }

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