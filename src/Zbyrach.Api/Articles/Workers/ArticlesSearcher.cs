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
        private readonly MediumTagsService _mediumTagsService;
        private readonly ILogger<ArticlesSearcher> _logger;

        public ArticlesSearcher(
            IServiceScopeFactory serviceScopeFactory,
            MediumTagsService mediumTagsService,
            ILogger<ArticlesSearcher> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
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
            if (!result.TopStories.Any())
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
                var externalId = GenerateId(story);

                var originalArticle = await articleService.GetByExternalId(externalId);
                if (originalArticle == null)
                {
                    originalArticle = await SaveArticle(articleService, story, externalId);
                    await articleService.SetStatus(originalArticle, users, ArticleStatus.New);
                    await articleService.LinkWithTag(originalArticle, tag);
                    _logger.LogInformation("Story was successfully saved: {story}", story);
                } 
                else
                {
                    _logger.LogInformation("Story was previously saved: {story}", story);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Can't save story {story} because of {exception}", story, e);
            }
        }

        private async Task<Article> SaveArticle(ArticleService articleService, StoryDto story, string externalId)
        {
            var newArticle = new Article
            {
                FoundAt = DateTime.UtcNow,
                ExternalId = externalId,
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

        private string GenerateId(StoryDto story)
        {
            return GetStringSha256Hash(story.Url.ToLower());
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