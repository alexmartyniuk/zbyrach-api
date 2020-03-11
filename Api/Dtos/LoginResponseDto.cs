namespace Api.Dtos
{
    public class LoginResponseDto
    {
        public string AuthToken { get; set; }
        public UserDto User { get; set; }
    }
}