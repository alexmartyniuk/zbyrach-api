using System.Collections.Generic;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Tags
{
    public class Tag : Entity
    {
        public string Name { get; set; }
        public ICollection<TagUser> TagUsers { get; set; }
        public ICollection<ArticleTag> ArticleTags { get; set; }

        public override string ToString() => Name;
    }
}