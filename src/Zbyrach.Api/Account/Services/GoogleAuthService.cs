using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Zbyrach.Api.Account
{
    public class GoogleAuthService
    {
        private readonly HttpClient _httpClient = default!;

        public GoogleAuthService(HttpClient client)
        {
            _httpClient = client;
        }

        protected GoogleAuthService()
        {

        }

        public virtual async Task<GoogleTokenInfo?> FindGoogleToken(string idToken, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                return JsonSerializer.Deserialize<GoogleTokenInfo>(response);
            }
            catch (HttpRequestException)
            {                
                return null;
            }
        }
    }
}