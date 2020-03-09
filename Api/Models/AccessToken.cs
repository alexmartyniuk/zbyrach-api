using System;

namespace Api.Models
{
    public class AccessToken
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public string Provider { get; set; }
        public DateTime ExpiredAt { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}