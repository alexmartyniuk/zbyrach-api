using System;

namespace Zbyrach.Api.Tags
{
    public class StoryDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime PublicatedAt { get; set; }
        public string Url { get; set; } = default!;
        public string? IllustrationUrl { get; set; }
        public AuthorDto Author { get; set; } = default!;
        public long CommentsCount { get; set; }
        public long LikesCount { get; set; }
        public string? ReadingTime { get; set; }
        
        public override string ToString() => $"{Title}";
    }
}