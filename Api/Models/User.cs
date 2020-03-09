using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }

        [JsonIgnore]
        public ICollection<AccessToken> AccessTokens { get; set; }
    }
}