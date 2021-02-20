namespace Zbyrach.Api.Account
{
    public record LoginResponseDto
    {
        public string Token = default!;
        public UserDto User = default!;
    }
}