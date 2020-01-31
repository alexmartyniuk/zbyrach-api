using System.Collections.Generic;

namespace MediumGrabber.Api.Dtos
{
    public class ArchiveDto
    {
        public IEnumerable<long> Years { get; set;}
        public IEnumerable<StoryDto> TopStories { get; set; }
    }
}