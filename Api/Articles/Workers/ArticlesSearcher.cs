using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediumGrabber.Api.Account;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediumGrabber.Api.Tags;

namespace MediumGrabber.Api.Articles
{
    public class ArticlesSearcher : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ArticlesSearcher(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory; 
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {         
                await CollectArticles(stoppingToken);       
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task CollectArticles(CancellationToken stoppingToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<UsersService>();                
                var mediumTagService = scope.ServiceProvider.GetRequiredService<MediumTagsService>();
                var articleService = scope.ServiceProvider.GetRequiredService<ArticleService>();

                var users = await userService.GetUsersWithTags();
                var tags = users
                    .SelectMany(u => u.TagUsers)
                    .Select(tu => tu.Tag.Name)                    
                    .Distinct();                                                    

                foreach(var tag in tags)
                {
                    var result = await mediumTagService.GetFullTagInfoByName(tag);
                    var topStories = result.TopStories;

                    foreach(var story in result.TopStories)
                    {    
                        System.Console.WriteLine(story.Url);                        
                    }
                }
            } 
        }
    }
}