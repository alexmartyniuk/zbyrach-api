using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Zbyrach.Api.Account.Dto;

namespace Zbyrach.Api.Account
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IMediator _mediator; 

        public AccountController(ILogger<AccountController> logger, IMediator mediator)
        {
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
            await _mediator.Send(new LogoutRequest());
            return Ok();
        }

        [HttpPost]
        [Route("/account/language")]
        public async Task<IActionResult> SetLanguage([FromBody] SetLanguageRequest request)
        {
            if (string.IsNullOrEmpty(request.Language) || (request.Language != "en" && request.Language != "uk"))
            {
                return BadRequest($"Unsupported language '{request.Language}'");
            }

            await _mediator.Send(request);
            return Ok();
        }
    }
}