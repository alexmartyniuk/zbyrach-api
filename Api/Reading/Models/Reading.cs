using System;
using MediumGrabber.Api.Account;
using MediumGrabber.Api.Articles;

namespace MediumGrabber.Api.Readings
{
    public class Reading
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long ArticleId { get; set; }
        public Article Article { get; set; }
        public DateTime ReadAt { get; set; }
        public string ReadTime { get; set; }
    }
}