using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using MediumGrabber.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MediumGrabber.Api.Controllers
{
    public class TagsController : ControllerBase
    {
        [HttpGet]
        [Route("/tags/{tagName}")]
        public async Task<TagFullDto> Get(string tagName)
        {
            var config = Configuration.Default.WithDefaultLoader();            
            var context = BrowsingContext.New(config);
            try
            {   
                var mainDocument = await context.OpenAsync($"https://medium.com/tag/{tagName}");                            
                var archiveDocument = await context.OpenAsync($"https://medium.com/tag/{tagName}/archive");    
                return GetFullTag(mainDocument, archiveDocument);  
            }
            catch(Exception e)
            {                
                throw new Exception($"Error during getting data from 'medium.com':\r\n {e}");
            }                   
        }

        private TagFullDto GetFullTag(IDocument mainDocument, IDocument archiveDocument)
        {
            var name = mainDocument
                .QuerySelector("h1.heading-title")
                .TextContent
                .Trim();
            var url = mainDocument.BaseUrl.ToString();
            var relatedTags = GetRelatedTags(mainDocument);
            var topStories = GetTopStories(mainDocument);
            var archive = GetArchive(archiveDocument);

            return new TagFullDto
            {
               Name = name,
               Url = url,
               RelatedTags = relatedTags, 
               TopStories = topStories,
               Archive = archive
            };
        }

        private ArchiveDto GetArchive(IDocument archiveDocument)
        {
            var years = archiveDocument
                .QuerySelectorAll("div.timebucket a")
                .Select(a => 
                    {
                        var text = a.TextContent.Trim();
                        return long.Parse(text);
                    });         
            var topStories = GetTopStories(archiveDocument);

            return new ArchiveDto
            {
                Years = years,
                TopStories = topStories
            };
        }

        private IEnumerable<StoryDto> GetTopStories(IDocument document)
        {
            return document
                .QuerySelectorAll("div.postArticle")
                .Select(article => GetStory(article)); 
        }

        private StoryDto GetStory(IElement article)
        {
            var author = GetAuthor(article);
            var title = article
                .QuerySelector("h3")
                .TextContent
                .Trim();
            string description = GetDescription(article);
            var illustrationUrl = article
                .QuerySelector("figure img")?
                .Attributes["src"]
                .Value;
            var date = article
                .QuerySelector("time")
                .Attributes["datetime"]
                .Value;
            var url = article
                .QuerySelector("div.postArticle-readMore a")
                .Attributes["href"]
                .Value;
            var commentsCount = article
                .QuerySelector("div.buttonSet a")?
                .TextContent
                .Trim();
            var likesCount = article
                .QuerySelector("div.multirecommend span button")?
                .TextContent
                .Trim();
            var readTime = article
                .QuerySelector("span.readingTime")
                .Attributes["title"]
                .Value;

            return new StoryDto
            {
                Author = author,
                Title = title,
                Description = description,
                Url = url,
                PublicatedAt = date,
                IllustrationUrl = illustrationUrl,
                CommentsCount = commentsCount,
                LikesCount = likesCount,
                ReadingTime = readTime
            };
        }

        private static string GetDescription(IElement article)
        {
            var descriptionNode = article.QuerySelector("h4");
            if (descriptionNode == null)
            {
                descriptionNode = article.QuerySelector("p.graf");                
            }

            return descriptionNode?
                .TextContent
                .Trim();
        }

        private AuthorDto GetAuthor(IElement article)
        {
            var name = article
                .QuerySelector("div.postMetaInline-authorLockup a")
                .TextContent
                .Trim();
            var avatarUrl = article
                .QuerySelector("div.postMetaInline-avatar img")
                .Attributes["src"]
                .Value;

            return new AuthorDto
            {
                Name = name,
                AvatarUrl = avatarUrl
            };
        }

        private IEnumerable<TagDto> GetRelatedTags(IDocument document)
        {                        
            return document
                .QuerySelectorAll("ul.tags li a")
                .Select(link => 
                    new TagDto
                    {
                        Name = link.TextContent.Trim(),
                        Url = link.Attributes["href"].Value
                    }
                ); 
        }
    }
}