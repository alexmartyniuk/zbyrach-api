namespace MediumGrabber.Api.Account
{
    public class GoogleToken
    {
        public string iss { get; set; }
        public string sub { get; set; }
        public string azp { get; set; }
        public string aud { get; set; }
        public string iat { get; set; }
        public string exp { get; set; }
        public string email { get; set; }
        public string email_verified { get; set; }
        public string name { get; set; }
        public string picture { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string locale { get; set; }
    }
}