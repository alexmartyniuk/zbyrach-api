using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace Zbyrach.Api.Tags
{
    public class MediumTagsService
    {
        private const string _baseUrl = "https://medium.com/";
        private readonly ILogger<MediumTagsService> _logger;
        private IBrowsingContext _context;

        public MediumTagsService(ILogger<MediumTagsService> logger)
        {
            var config = Configuration.Default.WithDefaultLoader();
            _context = BrowsingContext.New(config);
            _logger = logger;
        }

        public async Task<TagFullDto> GetFullTagInfoByName(string tagName)
        {
            try
            {
                var mainDocument = await _context.OpenAsync(_baseUrl + $"tag/{tagName}");
                var archiveDocument = await _context.OpenAsync(_baseUrl + $"tag/{tagName}/archive");
                return GetFullTag(mainDocument, archiveDocument);
            }
            catch (Exception e)
            {
                throw new Exception($"Error during getting data from 'medium.com' by tag '{tagName}':\r\n {e}");
            }
        }

        public async Task<TagDto?> GetShortTagInfoByName(string tagName)
        {
            try
            {
                var mainDocument = await _context.OpenAsync(_baseUrl + $"tag/{tagName}");
                return GetTag(mainDocument);
            }
            catch (Exception e)
            {
                throw new Exception($"Error during getting data from 'medium.com' by tag '{tagName}':\r\n {e}");
            }
        }

        public async Task<IEnumerable<TagDto>> GetRelatedTags(string tagName)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            try
            {
                var mainDocument = await context.OpenAsync(_baseUrl + $"tag/{tagName}");
                return GetRelatedTags(mainDocument);
            }
            catch (Exception e)
            {
                throw new Exception($"Error during getting data from 'medium.com' by tag '{tagName}':\r\n {e}");
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

        private TagDto? GetTag(IDocument mainDocument)
        {
            var title = mainDocument
                .QuerySelector("h1.heading-title");
            if (title == null)
            {
                return null;
            }

            return new TagDto
            {
                Name = title.TextContent.Trim(),
                Url = mainDocument.BaseUrl.ToString()
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
                ).ToList();
        }

        private IEnumerable<StoryDto> GetTopStories(IDocument document)
        {
            return document
                .QuerySelectorAll("div.postArticle")
                .Select(article => GetStory(article))
                .Where(story => story != null)
                .ToList()!;
        }

        private StoryDto? GetStory(IElement article)
        {
            try
            {
                return new StoryDto
                {
                    Author = GetAuthor(article),
                    Title = GetTitle(article),
                    Description = GetDescription(article),
                    Url = GetUrl(article),
                    PublicatedAt = GetPublicationDate(article),
                    IllustrationUrl = GetIllustrationUrl(article),
                    CommentsCount = GetCommentsCount(article),
                    LikesCount = GetLikesCount(article),
                    ReadingTime = GetReadTime(article)
                };
            }
            catch (Exception e)
            {
                _logger.LogError("Error during parsing article {pageUrl}: {error}", article.BaseUri.ToString(), e.Message);
                return null;
            }
        }

        private string? GetReadTime(IElement article)
        {
            return article
                .QuerySelector("span.readingTime")?
                .Attributes["title"]?
                .Value;
        }

        private long GetLikesCount(IElement article)
        {
            var value = article
                .QuerySelector("div.multirecommend span button")?
                .TextContent
                .Trim();
            return GetNumber(value);
        }

        private long GetCommentsCount(IElement article)
        {
            var value = article
                .QuerySelector("div.buttonSet a")?
                .TextContent
                .Trim();
            return GetNumber(value);
        }

        private DateTime GetPublicationDate(IElement article)
        {
            var value = article
                .QuerySelector("time")
                .Attributes["datetime"]
                .Value;
            return GetDateTime(value);
        }

        private string? GetIllustrationUrl(IElement article)
        {
            return article
                .QuerySelector("figure img")?
                .Attributes["src"]
                .Value;
        }

        private string NormalizeUrl(string url)
        {
            var builder = new UriBuilder(new Uri(url))
            {
                Query = string.Empty
            };
            return builder.Uri.ToString();
        }

        private DateTime GetDateTime(string value)
        {
            var result = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return result;
        }

        private long GetNumber(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            var mutiplicator = 1000;
            var value = GetByRegexp(text, @"(\d*)K", 1);
            if (string.IsNullOrWhiteSpace(value))
            {
                mutiplicator = 1;
                value = GetByRegexp(text, @"(\d*)\D", 1);
            }

            long.TryParse(value, out long result);
            return mutiplicator * result;
        }

        private string GetByRegexp(string input, string regexp, int group, string defaultValue = "")
        {
            var match = Regex.Match(input,
                regexp,
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return defaultValue;
            }

            return match.Groups[group].Value;
        }

        private string? GetTitle(IElement article)
        {
            var titleNode = article.QuerySelector("h3")
                ?? article.QuerySelector("p.graf-after--figure");

            return titleNode?
                .TextContent
                .Trim();
        }

        private string? GetDescription(IElement article)
        {
            var descriptionNode = article.QuerySelector("h4")
                ?? article.QuerySelector("p.graf-after--h3")
                ?? article.QuerySelector("p.graf-after--p");

            return descriptionNode?
                .TextContent
                .Trim();
        }

        private string GetUrl(IElement article)
        {
            var readMoreLink = article
                .QuerySelector("div.postArticle-readMore a");
            if (readMoreLink != null)
            {
                return NormalizeUrl(readMoreLink.Attributes["href"].Value);
            }

            var titleNode = article.QuerySelector("h3")
               ?? article.QuerySelector("p.graf-after--figure");
            var link = titleNode.Closest("a[href]");
            if (link != null)
            {
                return NormalizeUrl(link.Attributes["href"].Value);
            }

            throw new Exception($"Can't find url of the article.");
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
    }
}