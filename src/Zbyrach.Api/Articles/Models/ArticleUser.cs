using System;
using Zbyrach.Api.Account;

namespace Zbyrach.Api.Articles
{
    public class ArticleUser
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long ArticleId { get; set; }
        public Article Article { get; set; }
        public ArticleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime ReadAt { get; set; }
    }
}