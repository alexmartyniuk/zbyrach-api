namespace Api.Dtos
{
    public class LoginDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PictureUrl { get; set; }
        public string Provider { get; set; }
        public string AuthToken { get; set; }
        public string IdToken { get; set; }                 
    }
}