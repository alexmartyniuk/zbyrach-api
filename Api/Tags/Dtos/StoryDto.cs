namespace MediumGrabber.Api.Tags
{
    public class StoryDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PublicatedAt { get; set; }
        public string Url { get; set; }
        public string IllustrationUrl { get; set; }
        public AuthorDto Author { get; set; }
        public string CommentsCount { get; set; }
        public string LikesCount { get; set; }
        public string ReadingTime { get; set; }
    }
}