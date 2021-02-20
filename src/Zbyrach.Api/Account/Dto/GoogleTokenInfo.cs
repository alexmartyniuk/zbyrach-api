namespace Zbyrach.Api.Account
{
    public class GoogleTokenInfo
    {
        public string iss { get; set; } = default!;
        public string sub { get; set; } = default!;
        public string azp { get; set; } = default!;
        public string aud { get; set; } = default!;
        public string iat { get; set; } = default!;
        public string exp { get; set; } = default!;
        public string email { get; set; } = default!;
        public string email_verified { get; set; } = default!;
        public string name { get; set; } = default!;
        public string picture { get; set; } = default!;
        public string given_name { get; set; } = default!;
        public string family_name { get; set; } = default!;
        public string locale { get; set; } = default!;
    }
}