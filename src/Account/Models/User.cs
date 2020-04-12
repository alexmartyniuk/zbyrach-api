using System.Collections.Generic;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Tags;

namespace Zbyrach.Api.Account
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }
        public ICollection<AccessToken> AccessTokens { get; set; }
        public ICollection<TagUser> TagUsers { get; set; }
        public MailingSettings MailingSettings { get; set; }
        public ICollection<ArticleUser> ArticleUsers { get; set; }
        public override string ToString() => Email;
    }
}