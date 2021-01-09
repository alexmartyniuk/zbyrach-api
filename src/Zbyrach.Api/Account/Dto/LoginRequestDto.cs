using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Zbyrach.Api.Account
{
    public class LoginRequestDto : IRequest<LoginResponseDto>
    {
        [Required]
        public string Token { get; set; }
    }
}