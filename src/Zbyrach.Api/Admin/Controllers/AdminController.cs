using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zbyrach.Api.Account;
using Zbyrach.Api.Admin.Services;
using Zbyrach.Api.Articles;
using StatisticResponse = Zbyrach.Api.Admin.Dto.StatisticResponse;

namespace Zbyrach.Api.Admin.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UsersService _userService;
        private readonly AdminService _adminService;
        private readonly PdfService _pdfService;

        public AdminController(UsersService accountService, AdminService adminService, PdfService pdfService)
        {
            _userService = accountService;
            _adminService = adminService;
            _pdfService = pdfService;
        }

        [HttpGet]
        [Route("/statistic")]
        public async Task<IActionResult> Statistic()
        {
            var user = await _userService.GetCurrent();
            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var totalSizeInBytes = await _adminService.GetTotalSizeInBytes();
            var totalRowsCount = await _adminService.GetTotalRowsCount();
            var articlesCount = await _adminService.GetArticlesCount();
            var usersCount = await _adminService.GetUsersCount();
            var tagsCount = await _adminService.GetTagsCount();
            var resp = await _pdfService.GetStatistic();
            
            return Ok(new StatisticResponse
            {
                ArticlesCount = articlesCount,
                TagsCount = tagsCount,
                UsersCount = usersCount,
                DbTotalRowsCount = totalRowsCount,
                DbTotalSizeInBytes = totalSizeInBytes,
                PdfCashDataSize = resp.TotalSizeInBytes,
                PdfCashItemsCount = resp.TotalRowsCount,
            });
        }

        [HttpDelete]
        [Route("/cleanup/{daysCleanup}")]
        public async Task<IActionResult> Cleanup([FromRoute] int daysCleanup)
        {
            var user = await _userService.GetCurrent();
            if (!user.IsAdmin)
            {
                return Forbid();
            }

            if (daysCleanup < 1)
            {
                return BadRequest();
            }
            
            await _pdfService.Cleanup(daysCleanup);
            await _adminService.Cleanup(daysCleanup);

            return NoContent();
        }
    }
}