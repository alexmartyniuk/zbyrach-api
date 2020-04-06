using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zbyrach.Api.Account;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zbyrach.Api.Tags;
using System.IO;

namespace Zbyrach.Api.Articles
{
    public class ArticlesSearcher : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly FileService _fileService;
        private readonly PdfService _pdfService;

        public ArticlesSearcher(
            IServiceScopeFactory serviceScopeFactory,
            FileService fileService,
            PdfService pdfService)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _fileService = fileService;
            _pdfService = pdfService;
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
            var mediumTagService = scope.ServiceProvider.GetRequiredService<MediumTagsService>();
            var articleService = scope.ServiceProvider.GetRequiredService<ArticleService>();

            // TODO: move to tags service
            var users = await userService.GetUsersWithTags();
            var tags = users
                .SelectMany(u => u.TagUsers)
                .Select(tu => tu.Tag)
                .Distinct(); //TODO: add comparer to distinct tags

            foreach (var tag in tags)
            {
                var result = await mediumTagService.GetFullTagInfoByName(tag.Name);
                var topStories = result.TopStories;  
                foreach (var story in result.TopStories)
                {
                    await SaveArticleIfNeeded(articleService, story, tag);
                }
            }
        }

        private async Task SaveArticleIfNeeded(ArticleService articleService, StoryDto story, Tag tag)
        {
            var externalId = GetId(story.Url);

            var originalArticle = await articleService.GetByExternalIdWithTags(externalId);
            if (originalArticle == null)
            {
                originalArticle = await SaveArticle(articleService, story, externalId);   
            } 
            
            if (!originalArticle.ArticleTags
                .Select(at => at.Tag.Name)
                .Contains(tag.Name))
            {
                originalArticle.ArticleTags.Add(new ArticleTag
                {
                    ArticleId = originalArticle.Id,
                    TagId = tag.Id,
                });
                await articleService.UpdateOne(originalArticle);
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
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            using var sha = new System.Security.Cryptography.SHA256Managed();
            byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] hash = sha.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }
}