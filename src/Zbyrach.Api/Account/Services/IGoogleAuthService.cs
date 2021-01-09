using System.Threading.Tasks;

namespace Zbyrach.Api.Account
{
    public interface IGoogleAuthService
    {
        Task<GoogleToken> FindGoogleToken(string idToken);
    }
}