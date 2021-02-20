using System.Collections.Generic;

namespace Zbyrach.Api.Tags
{
    public class TagFullDto : TagDto
    {
        public IEnumerable<TagDto> RelatedTags { get; set; } = default!;
        public IEnumerable<StoryDto> TopStories { get; set; } = default!;
        public ArchiveDto Archive { get; set; } = default!;
    }
}