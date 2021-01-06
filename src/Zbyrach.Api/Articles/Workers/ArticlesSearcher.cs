using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zbyrach.Api.Account;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zbyrach.Api.Tags;
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
                    _logger.LogError(e, "Unhandled exception during collecting articles");
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task CollectArticles(CancellationToken stoppingToken)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var tagService = serviceScope.ServiceProvider.GetRequiredService<TagService>();

            var tagsForSearch = await tagService.GetTagsWithUsers();
            foreach (var pair in tagsForSearch)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                var tag = pair.Key;
                var users = pair.Value;

                try
                {
                    await FindAndSaveArticles(serviceScope, tag, users, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unexpected error during searching for tag {tag}");
                }
            }
        }

        private async Task FindAndSaveArticles(IServiceScope serviceScope, Tag tag, List<User> users, CancellationToken stoppingToken)
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

                await SaveArticle(serviceScope, story, tag, users);
            }
        }

        private async Task SaveArticle(IServiceScope serviceScope, StoryDto story, Tag tag, List<User> users)
        {
            try
            {
                var articleService = serviceScope.ServiceProvider.GetRequiredService<ArticleService>();
                var translationService = serviceScope.ServiceProvider.GetRequiredService<TranslationService>();
                
                var textToDetect = story.Description ?? story.Title;
                var language = translationService.DetectLanguage(textToDetect);
                var newArticle = CreateArticle(serviceScope, story, language);

                var savedArticle = await articleService.SaveArticle(newArticle, users, tag);           
                
                _logger.LogInformation("Story was successfully saved: {story}, article id: {articleId}", story, savedArticle.Id);                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Can't save story {story} because of {exception}", story, e.Message);
            }
        }

        private Article CreateArticle(IServiceScope serviceScope, StoryDto story, string language)
        {
            return new Article
            {
                FoundAt = DateTime.UtcNow,
                PublicatedAt = story.PublicatedAt,
                IllustrationUrl = story.IllustrationUrl,
                Description = story.Description,
                ReadTime = story.ReadingTime,
                Title = story.Title,
                Language = language,
                Url = story.Url,
                LikesCount = story.LikesCount,
                CommentsCount = story.CommentsCount,
                AuthorName = story.Author.Name,
                AuthorPhoto = story.Author.AvatarUrl
            };
        }
    }
}