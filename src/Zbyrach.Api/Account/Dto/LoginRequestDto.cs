using System.ComponentModel.DataAnnotations;

namespace Zbyrach.Api.Account
{
    public class LoginRequestDto
    {
        [Required]
        public string Token { get; set; }
    }
}