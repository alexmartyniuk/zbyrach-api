using System.Linq;
using System.Threading.Tasks;
using Zbyrach.Api.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Zbyrach.Api.Mailing
{
    [Authorize]
    public class MailingSettingsController : Controller
    {
        private readonly UsersService _userService;
        private readonly MailingSettingsService _mailingSettingService;
        private readonly CronService _cronService;

        public MailingSettingsController(
            UsersService userService,
            MailingSettingsService mailingSettingService,
            CronService cronService)
        {
            _userService = userService;
            _mailingSettingService = mailingSettingService;
            _cronService = cronService;
        }

        [HttpGet]
        [Route("/mailing/settings/my")]
        public async Task<IActionResult> GetMySettings()
        {
            var currentUser = await _userService.GetCurrentUser();

            var settings = await _mailingSettingService.GetByUser(currentUser);
            return Ok(new MailingSettingsDto
            {
                ScheduleType = _cronService.ExpressionToSchedule(settings.Schedule),
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
                Schedule = _cronService.ScheduleToExpression(settings.ScheduleType)
            });

            var savedSettings = await _mailingSettingService.GetByUser(currentUser);

            return Ok(new MailingSettingsDto
            {
                ScheduleType = _cronService.ExpressionToSchedule(savedSettings.Schedule),
                NumberOfArticles = savedSettings.NumberOfArticles
            });
        }
    }
}