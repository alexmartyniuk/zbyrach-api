using System.Collections.Generic;

namespace MediumGrabber.Api.Tags
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name {get; set;}
        public ICollection<TagUser> TagUsers { get; set; }

        public override string ToString() => Name;
    }
}