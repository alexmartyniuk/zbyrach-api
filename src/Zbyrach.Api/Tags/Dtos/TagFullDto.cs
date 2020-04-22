using System.Collections.Generic;

namespace Zbyrach.Api.Tags
{
    public class TagFullDto : TagDto
    {
        public IEnumerable<TagDto> RelatedTags { get; set; }
        public IEnumerable<StoryDto> TopStories { get; set; }
        public ArchiveDto Archive { get; set; }
    }
}