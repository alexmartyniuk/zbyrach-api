using Zbyrach.Api.Account;

namespace Zbyrach.Api.Tags
{
    public class TagUser
    {
        public long TagId { get; set; }
        public Tag Tag { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}