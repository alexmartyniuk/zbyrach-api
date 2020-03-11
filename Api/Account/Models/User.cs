using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediumGrabber.Api.Account
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }

        public ICollection<AccessToken> AccessTokens { get; set; }
    }
}