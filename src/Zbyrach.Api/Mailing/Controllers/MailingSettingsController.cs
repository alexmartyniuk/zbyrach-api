using System.Linq;
using System.Threading.Tasks;
using Zbyrach.Api.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zbyrach.Api.Tags;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Mailing
{
    [Authorize]
    public class MailingSettingsController : Controller
    {
        private readonly UsersService _userService;
        private readonly MailingSettingsService _mailingSettingService;
        private readonly CronService _cronService;
        private readonly TagService _tagsService;
        private readonly MailService _mailService;
        private readonly ArticleService _articleService;

        public MailingSettingsController(
            UsersService userService,
            MailingSettingsService mailingSettingService,
            CronService cronService,
            TagService tagsService,
            MailService mailService,
            ArticleService articleService)
        {
            _userService = userService;
            _mailingSettingService = mailingSettingService;
            _cronService = cronService;
            _tagsService = tagsService;
            _mailService = mailService;
            _articleService = articleService;
        }

        [HttpGet]
        [Route("/mailing/settings/my")]
        public async Task<IActionResult> GetMySettings()
        {
            var currentUser = await _userService.GetCurrent();

            var settings = await _mailingSettingService.Get(currentUser);
            return Ok(new MailingSettingsDto
            {
                ScheduleType = _cronService.ExpressionToSchedule(settings.Schedule),
                NumberOfArticles = settings.NumberOfArticles
            });
        }

        [HttpGet]
        [Route("/mailing/settings/summary")]
        public async Task<IActionResult> GetSettingsSummary()
        {
            var currentUser = await _userService.GetCurrent();

            var mailSettings = await _mailingSettingService.Get(currentUser);
            var tagsCount = await _tagsService.GetTagsCountByUser(currentUser);
            return Ok(new SettingsSummaryDto
            {
                ScheduleType = _cronService.ExpressionToSchedule(mailSettings.Schedule),
                NumberOfTags = tagsCount
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

            var currentUser = await _userService.GetCurrent();

            await _mailingSettingService.CreateOrUpdate(currentUser, new MailingSettings
            {
                NumberOfArticles = settings.NumberOfArticles,
                Schedule = _cronService.ScheduleToExpression(settings.ScheduleType)
            });

            var savedSettings = await _mailingSettingService.Get(currentUser);

            return Ok(new MailingSettingsDto
            {
                ScheduleType = _cronService.ExpressionToSchedule(savedSettings.Schedule),
                NumberOfArticles = savedSettings.NumberOfArticles
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/mailing/unsubscribe/{token}")]
        public async Task<IActionResult> Unsubscribe([FromRoute] string token)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.First().Errors;
                return BadRequest(new JsonResult(errors));
            }

            var currentUser = await _userService.GetUserByUnsubscribeToken(token);
            if (currentUser == null)
            {
                return BadRequest("Invalid token");
            }

            await _mailingSettingService.UnsubscribeUser(currentUser);

            return Ok(new UserDto
            {
                Id = currentUser.Id,
                Email = currentUser.Email,
                Name = currentUser.Name,
                PictureUrl = currentUser.PictureUrl,
                IsAdmin = currentUser.IsAdmin,
                Language = currentUser.Language
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/mailing/test/{email}")]
        public async Task<IActionResult> Test([FromRoute] string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var articles = await _articleService.GetForReading(user);
            await _mailService.SendArticleList(user, string.Empty, articles, ScheduleType.EveryMonth);

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                PictureUrl = user.PictureUrl,
                Language = user.Language,
                IsAdmin = user.IsAdmin
            });
        }
    }
}