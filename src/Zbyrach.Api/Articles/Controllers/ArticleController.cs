using System;
using System.Collections.Generic;
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
using Wangkanai.Detection.Services;

namespace Zbyrach.Api.Articles
{
    [Authorize]
    public class ArticleController : Controller
    {
        private readonly ArticleService _articleService;
        private readonly UsersService _userService;
        private readonly PdfService _pdfService;
        private readonly MailingSettingsService _mailingSettingsService;
        private readonly IDetectionService _detectionService;

        public ArticleController(ArticleService articleService, 
            UsersService userService, 
            PdfService pdfService, 
            MailingSettingsService mailingSettingsService,
            IDetectionService detectionService)
        {
            _articleService = articleService;
            _userService = userService;
            _pdfService = pdfService;
            _mailingSettingsService = mailingSettingsService;
            _detectionService = detectionService;
        }

        [HttpGet]
        [Route("/articles/status/read")]
        public async Task<IActionResult> GetArticlesForReading()
        {
            var currentUser = await _userService.GetCurrent();
            var articles = await _articleService.GetForReading(currentUser);
            var articleUsers = await _articleService.GetArticleUsers(currentUser, articles);

            var articlesDtos = articles
                .Select(a =>
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
                    ReadTime = a.ReadTime,
                    Favorite = articleUsers.Single(au => au.ArticleId == a.Id).Favorite,
                    ReadLater = articleUsers.Single(au => au.ArticleId == a.Id).ReadLater,
                    Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList()
                });
            return Ok(articlesDtos);
        }

        [HttpGet]
        [Route("/articles/status/sent")]
        public async Task<IActionResult> GetLastSentArticles()
        {
            var currentUser = await _userService.GetCurrent();            
            var articles = await _articleService.GetLastSent(currentUser);

            var articlesDtos = articles
                .Select(a =>
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
                    ReadTime = a.ReadTime,
                    Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList()
                });
            return Ok(articlesDtos);
        }

        [HttpPost]
        [Route("/articles/{articleId}/favorite/{favorite}")]
        public async Task<IActionResult> SetFavorite(long articleId, bool favorite)
        {
            var currentUser = await _userService.GetCurrent();
            var article = await _articleService.FindById(articleId);
            var newArticle = await _articleService.SetFavorite(currentUser, article, favorite);
            var articleUser = await _articleService.GetArticleUser(currentUser, article);

            var articlesDto = new ArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Description = article.Description,
                PublicatedAt = article.PublicatedAt,
                IllustrationUrl = article.IllustrationUrl,
                OriginalUrl = article.Url,
                AuthorName = article.AuthorName,
                AuthorPhoto = article.AuthorPhoto,
                CommentsCount = article.CommentsCount,
                LikesCount = article.LikesCount,
                ReadTime = article.ReadTime,
                Favorite = articleUser.Favorite,
                ReadLater = articleUser.ReadLater,
                Tags = article.ArticleTags.Select(at => at.Tag.Name).ToList()
            };
            return Ok(articlesDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/articles/{articleId}/pdf")]
        public async Task<IActionResult> GetPdf(long articleId, [FromQuery] long userId = 0, [FromQuery] bool inline = false)
        {
            var article = await _articleService.FindById(articleId);
            if (article == null)
            {
                return NotFound();
            }

            var stream = await _pdfService.ConvertUrlToPdf(article.Url, _detectionService.Device.Type, inline);
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
            {
                FileName = GetPdfFileName(article.Url),
                DispositionType = inline ? DispositionTypeNames.Inline : DispositionTypeNames.Attachment
            }.ToString();

            var user = await _userService.FindById(userId);
            if (user != null)
            {
                await _articleService.MarkAsRead(article, user);
            }

            return File(stream, "application/pdf");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/articles/status/read/send/test/{userEmail}")]
        public async Task<IActionResult> SendNewArticleEvent(string userEmail)
        {
            var user = await _userService.GetUserByEmail(userEmail);
            if (user == null)
            {
                return NotFound("User not found by email");
            }

            var articles = await _articleService.GetForReading(user);
            var random = new Random();
            var number = random.Next(0, articles.Count - 1);
            var article = articles.ElementAtOrDefault(number);
            if (article == null)
            {
                return NotFound("No articles for reading");
            }
            await _articleService.SendNewArticleEvent(article, new List<User> {user});
            
            return NoContent();
        }

        private string GetPdfFileName(string url)
        {
            var uri = new Uri(url.ToLower());
            var fileName = Path.GetFileName(uri.LocalPath) + ".pdf";
            return WebUtility.UrlEncode(fileName);
        }
    }
}