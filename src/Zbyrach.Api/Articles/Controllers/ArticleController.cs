using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Zbyrach.Api.Account;
using Zbyrach.Api.Mailing;
using System.Net;

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
        [AllowAnonymous]
        [Route("/articles/{articleId}/pdf")]
        public async Task<IActionResult> GetPdf(long articleId, [FromQuery] long userId = 0, [FromQuery] bool inline = false)
        {
            var article = await _articleService.GetById(articleId);
            if (article == null)
            {
                return NotFound();
            }

            var stream = await _pdfService.ConvertUrlToPdf(article.Url, inline);
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
            {
                FileName = GetPdfFileName(article.Url),
                DispositionType = inline ? DispositionTypeNames.Inline : DispositionTypeNames.Attachment 
            }.ToString();

            var user = await _userService.GetById(userId);
            if (user != null)
            {
                await _articleService.MarkAsRead(article, user);
            }

            return File(stream, "application/pdf");
        }

        private string GetPdfFileName(string url)
        {
            var uri = new Uri(url.ToLower());
            var fileName = Path.GetFileName(uri.LocalPath) + ".pdf";
            return WebUtility.UrlEncode(fileName);
        }
    }
}