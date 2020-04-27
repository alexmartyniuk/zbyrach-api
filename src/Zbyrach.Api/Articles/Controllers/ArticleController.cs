using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zbyrach.Api.Account;
using Zbyrach.Api.Mailing;

namespace Zbyrach.Api.Articles
{
    [Authorize]
    public class ArticleController : Controller
    {
        private readonly ArticleService _articleService;
        private readonly UsersService _userService;
        private readonly FileService _fileService;
        private readonly MailingSettingsService _mailingSettingsService;

        public ArticleController(ArticleService articleService, UsersService userService, FileService fileService, MailingSettingsService mailingSettingsService)
        {
            _articleService = articleService;
            _userService = userService;
            _fileService = fileService;
            _mailingSettingsService = mailingSettingsService;
        }

        [HttpGet]
        [Route("/articles/status/read")]
        public async Task<IActionResult> GetArticlesForRead()
        {
            var currentUser = await _userService.GetCurrentUser();
            var mailingSettings = await _mailingSettingsService.Get(currentUser);
            var noMoreThan = mailingSettings?.NumberOfArticles ?? 0;

            var articles = await _articleService.GetForReading(currentUser);
            var articlesDtos = articles.Select(a =>            
                new ArticleDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    PublicatedAt = a.PublicatedAt,
                    IllustrationUrl = a.IllustrationUrl,
                    OriginalUrl = a.Url,
                    AuthorName = a.AuthorName,
                    AuthorPhoto = a.AuthorPhoto,
                    CommentsCount = a.CommentsCount,
                    LikesCount = a.LikesCount,
                    ReadTime = a.ReadTime
                });
            return Ok(articlesDtos);
        }

        [HttpGet]
        [Route("/articles/pdf/{articleId}")]
        public async Task<IActionResult> GetPdf(long articleId)
        {            
            // TODO: check that article belongs to user
            var article = await _articleService.GetById(articleId);
            if (article == null)
            {
                return NotFound();
            }

            var stream = await _fileService.GetFile(article.FileName);
            return File(stream, "application/pdf", article.FileName);                    
        }
    }
}