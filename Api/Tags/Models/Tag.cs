using System;
using MediumGrabber.Api.Account;

namespace MediumGrabber.Api.Tags
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name {get; set;}
        public long UserId { get; set; }
        public User User { get; set; }
    }
}