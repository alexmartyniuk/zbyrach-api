using System;

namespace Zbyrach.Api.Account
{
    public class AccessToken : Entity
    {
        public string ClientIp { get; set; }
        public string ClientUserAgent { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }

        public DateTime ExpiredAt()
        {
            return CreatedAt + TimeSpan.FromDays(30);
        }
    }
}