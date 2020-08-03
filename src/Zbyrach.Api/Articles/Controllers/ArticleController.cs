using System;
using System.IO;
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
        private readonly PdfService _pdfService;
        private readonly MailingSettingsService _mailingSettingsService;

        public ArticleController(ArticleService articleService, UsersService userService, PdfService pdfService, MailingSettingsService mailingSettingsService)
        {
            _articleService = articleService;
            _userService = userService;
            _pdfService = pdfService;
            _mailingSettingsService = mailingSettingsService;
        }

        [HttpGet]
        [Route("/articles/status/read")]
        public async Task<IActionResult> GetArticlesForRead()
        {
            var currentUser = await _userService.GetCurrentUser();
            var mailingSettings = await _mailingSettingsService.Get(currentUser);
            var noMoreThan = mailingSettings?.NumberOfArticles ?? 0;

            var articleTags = await _articleService.GetForReading(currentUser);
            var articlesDtos = articleTags
                .GroupBy(at => at.Article)
                .Select(g =>            
                new ArticleDto
                {
                    Id = g.Key.Id,
                    Title = g.Key.Title,
                    Description = g.Key.Description,
                    PublicatedAt = g.Key.PublicatedAt,
                    IllustrationUrl = g.Key.IllustrationUrl,
                    OriginalUrl = g.Key.Url,
                    AuthorName = g.Key.AuthorName,
                    AuthorPhoto = g.Key.AuthorPhoto,
                    CommentsCount = g.Key.CommentsCount,
                    LikesCount = g.Key.LikesCount,
                    ReadTime = g.Key.ReadTime,
                    Tags = g.Select(at => at.Tag.Name).ToList()
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

            var stream = await _pdfService.ConvertUrlToPdf(article.Url);
            var fileName = GetPdfFileName(article.Url);
            return File(stream, "application/pdf", fileName);                    
        }

        private string GetPdfFileName(string url)
        {
            var uri = new Uri(url.ToLower());
            return Path.GetFileName(uri.LocalPath) + ".pdf";
        }
    }
}