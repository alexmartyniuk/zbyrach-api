using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediumGrabber.Api.Account;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediumGrabber.Api.Tags;
using System.IO;

namespace MediumGrabber.Api.Articles
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
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task CollectArticles(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UsersService>();
            var mediumTagService = scope.ServiceProvider.GetRequiredService<MediumTagsService>();
            var articleService = scope.ServiceProvider.GetRequiredService<ArticleService>();

            var users = await userService.GetUsersWithTags();
            var tags = users
                .SelectMany(u => u.TagUsers)
                .Select(tu => tu.Tag.Name)
                .Distinct();

            foreach (var tag in tags)
            {
                var result = await mediumTagService.GetFullTagInfoByName(tag);
                var topStories = result.TopStories;

                foreach (var story in result.TopStories)
                {
                    await SaveArticle(story);
                }
            }
        }

        private async Task SaveArticle(StoryDto story)
        {
            var fileName = GetArticleFileName(story.Url);
            fileName = Path.ChangeExtension(fileName, ".pdf");

            if (!_fileService.IsFileExists(fileName))
            {
                var stream = await _pdfService.ConvertUrlToPdf(story.Url);
                await _fileService.PutFile(fileName, stream);
            }
            else
            {
                System.Console.WriteLine($"File already exists: {fileName}");
            }
        }

        private string GetArticleFileName(string url)
        {
            var uri = new Uri(url.ToLower());
            return Path.GetFileName(uri.LocalPath);
        }
    }
}