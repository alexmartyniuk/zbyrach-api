using System.Collections.Generic;
using MediumGrabber.Api.Articles;

namespace MediumGrabber.Api.Tags
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name {get; set;}
        public ICollection<TagUser> TagUsers { get; set; }
        public ICollection<ArticleTag> ArticleTags { get; set; }

        public override string ToString() => Name;
    }
}