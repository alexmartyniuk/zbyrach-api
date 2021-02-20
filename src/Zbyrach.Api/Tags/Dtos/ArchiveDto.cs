using System.Collections.Generic;

namespace Zbyrach.Api.Tags
{
    public class ArchiveDto
    {
        public IEnumerable<long> Years { get; set; } = default!;
        public IEnumerable<StoryDto> TopStories { get; set; } = default!;
    }
}