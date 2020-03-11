namespace MediumGrabber.Api.Account
{
    public class LoginResponseDto
    {
        public string AuthToken { get; set; }
        public UserDto User { get; set; }
    }
}