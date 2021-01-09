using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zbyrach.Api.Account
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(ILogger<GoogleAuthService> logger)
        {
            _logger = logger;
        }

        public async Task<GoogleToken> FindGoogleToken(string idToken)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                return JsonSerializer.Deserialize<GoogleToken>(response);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Google token could not be validated.");
                return null;
            }
        }
    }
}