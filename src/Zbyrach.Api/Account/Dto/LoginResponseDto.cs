namespace Zbyrach.Api.Account
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = default!;
        public UserDto User { get; set; } = default!;
    }
}