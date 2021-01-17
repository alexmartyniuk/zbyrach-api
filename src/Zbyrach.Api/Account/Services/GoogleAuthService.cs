using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zbyrach.Api.Account
{
    public class GoogleAuthService
    {
        private readonly HttpClient _httpClient;

        public GoogleAuthService(HttpClient client)
        {
            _httpClient = client;
        }

        public virtual async Task<GoogleToken> FindGoogleToken(string idToken)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                return JsonSerializer.Deserialize<GoogleToken>(response);
            }
            catch (HttpRequestException)
            {                
                return null;
            }
        }
    }
}