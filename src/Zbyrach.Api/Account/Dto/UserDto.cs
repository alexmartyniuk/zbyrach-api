namespace Zbyrach.Api.Account
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PictureUrl { get; set; } = default!;
        public bool IsAdmin { get; set; } = default!;
        public string Language { get; set; } = default!;
    }
}