using System.Linq;
using System.Threading.Tasks;
using MediumGrabber.Api.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediumGrabber.Api.Mailing
{
    [Authorize]
    public class MailingSettingsController : Controller
    {
        private readonly UsersService _userService;
        private readonly MailingSettingsService _mailingSettingService;

        public MailingSettingsController(UsersService userService, MailingSettingsService mailingSettingService)
        {
            _userService = userService;
            _mailingSettingService = mailingSettingService;
        }

        [HttpGet]
        [Route("/mailing/settings/my")]
        public async Task<IActionResult> GetMySettings()
        {
            var currentUser = await _userService.GetCurrentUser();

            var settings = await _mailingSettingService.GetByUser(currentUser);
            return Ok(new MailingSettingsDto
            {
                Schedule = settings.Schedule,
                NumberOfArticles = settings.NumberOfArticles
            });
        }

        [HttpPost]
        [Route("/mailing/settings/my")]
        public async Task<IActionResult> SetMySettings([FromBody] MailingSettingsDto settings)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.First().Errors;
                return BadRequest(new JsonResult(errors));
            }

            var currentUser = await _userService.GetCurrentUser();

            await _mailingSettingService.SetByUser(currentUser, new MailingSettings
            {
                NumberOfArticles = settings.NumberOfArticles,
                Schedule = settings.Schedule
            });

            var savedSettings = await _mailingSettingService.GetByUser(currentUser);
                        
            return Ok(savedSettings);
        }
    }
}