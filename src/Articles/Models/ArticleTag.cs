using MediumGrabber.Api.Tags;

namespace MediumGrabber.Api.Articles
{
    public class ArticleTag
    {    
        public long TagId { get; set; }
        public Tag Tag { get; set; }
        public long ArticleId { get; set; }
        public Article Article { get; set; }        
    }
}