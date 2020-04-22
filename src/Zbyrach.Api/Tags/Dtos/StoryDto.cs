using System;

namespace Zbyrach.Api.Tags
{
    public class StoryDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublicatedAt { get; set; }
        public string Url { get; set; }
        public string IllustrationUrl { get; set; }
        public AuthorDto Author { get; set; }
        public long CommentsCount { get; set; }
        public long LikesCount { get; set; }
        public string ReadingTime { get; set; }
    }
}