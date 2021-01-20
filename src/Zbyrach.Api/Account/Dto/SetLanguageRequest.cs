using MediatR;

namespace Zbyrach.Api.Account
{
    public class SetLanguageRequest : IRequest
    {
        public string Language { get; set; }
    }
}