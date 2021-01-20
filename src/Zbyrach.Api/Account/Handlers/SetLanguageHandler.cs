using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Zbyrach.Api.Account.Handlers
{
    public class SetLanguageHandler : AsyncRequestHandler<SetLanguageRequest>
    {
        private readonly UsersService _usersService;

        public SetLanguageHandler(UsersService usersService)
        {
            _usersService = usersService;
        }

        protected override async Task Handle(SetLanguageRequest request, CancellationToken cancellationToken)
        {
            await _usersService.SetLanguage(request.Language);
        }
    }
}
