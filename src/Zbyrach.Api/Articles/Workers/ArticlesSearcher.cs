using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zbyrach.Api.Account;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zbyrach.Api.Tags;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Zbyrach.Api.Articles
{
    public class ArticlesSearcher : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly FileService _fileService;
        private readonly PdfService _pdfService;
        private readonly MediumTagsService _mediumTagsService;
        private readonly ILogger<ArticlesSearcher> _logger;

        public ArticlesSearcher(
            IServiceScopeFactory serviceScopeFactory,
            FileService fileService,
            PdfService pdfService,
            MediumTagsService mediumTagsService,
            ILogger<ArticlesSearcher> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _fileService = fileService;
            _pdfService = pdfService;
            _mediumTagsService = mediumTagsService;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectArticles(stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task CollectArticles(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UsersService>();
            var articleService = scope.ServiceProvider.GetRequiredService<ArticleService>();
            var tagService = scope.ServiceProvider.GetRequiredService<TagService>();

            var tagsForSearch = await tagService.GetTagsWithUsers();
            foreach (var pair in tagsForSearch)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                var tag = pair.Key;
                var users = pair.Value;

                await FindAndSaveArticles(articleService, tag, users, stoppingToken);
            }
        }

        private async Task FindAndSaveArticles(ArticleService articleService, Tag tag, List<User> users, CancellationToken stoppingToken)
        {
            var result = await _mediumTagsService.GetFullTagInfoByName(tag.Name);
            if (result.TopStories.Count() == 0)
            {
                _logger.LogWarning("No stories found by tag '{tagName}'", tag.Name);
            }
            else
            {
                _logger.LogInformation("Found {storiesCount} stories by tag '{tagName}'", result.TopStories.Count(), tag.Name);
            }

            foreach (var story in result.TopStories)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await SaveArticleIfNeeded(articleService, story, tag, users);
            }
        }

        private async Task SaveArticleIfNeeded(ArticleService articleService, StoryDto story, Tag tag, List<User> users)
        {
            try
            {
                var externalId = GetId(story.Url);

                var originalArticle = await articleService.GetByExternalId(externalId);
                if (originalArticle == null)
                {
                    originalArticle = await SaveArticle(articleService, story, externalId);
                    await articleService.SetStatus(originalArticle, users, ArticleStatus.New);
                    await articleService.LinkWithTag(originalArticle, tag);
                }
                _logger.LogInformation("Story {story} was successfully saved.", story);
            }
            catch (Exception e)
            {
                _logger.LogError("Can't save story {story} because of {exception}", story, e);
            }
        }

        private async Task<Article> SaveArticle(ArticleService articleService, StoryDto story, string externalId)
        {
            var fileName = await SavePdf(story.Url);
            var newArticle = new Article
            {
                FoundAt = DateTime.UtcNow,
                ExternalId = externalId,
                FileName = fileName,
                PublicatedAt = story.PublicatedAt,
                IllustrationUrl = story.IllustrationUrl,
                Description = story.Description,
                ReadTime = story.ReadingTime,
                Title = story.Title,
                Url = story.Url,
                LikesCount = story.LikesCount,
                CommentsCount = story.CommentsCount,
                AuthorName = story.Author.Name,
                AuthorPhoto = story.Author.AvatarUrl
            };

            return await articleService.SaveOne(newArticle);
        }

        private async Task<string> SavePdf(string url)
        {
            var fileName = GetFileName(url);
            fileName = Path.ChangeExtension(fileName, ".pdf");

            if (_fileService.IsFileExists(fileName))
            {
                throw new Exception($"File already exists: {fileName}");
            }

            var stream = await _pdfService.ConvertUrlToPdf(url);
            await _fileService.PutFile(fileName, stream);

            return fileName;
        }

        private string GetFileName(string url)
        {
            var uri = new Uri(url.ToLower());
            return Path.GetFileName(uri.LocalPath);
        }

        private string GetId(string url)
        {
            return GetStringSha256Hash(url.ToLower());
        }

        private string GetStringSha256Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            using var sha = new System.Security.Cryptography.SHA256Managed();
            byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] hash = sha.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }
}