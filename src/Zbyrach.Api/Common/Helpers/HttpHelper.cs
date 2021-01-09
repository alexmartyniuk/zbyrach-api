using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zbyrach.Api.Common.Helpers
{
    public static class HttpHelper
    {
        public static Task<HttpResponseMessage> PostJson<T>(this HttpClient httpClient, string requestUri, T payload)
        {
            var options = new JsonSerializerOptions
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var content = new StringContent(
               JsonSerializer.Serialize(payload, options),
               Encoding.UTF8,
               "application/json"
               );
            return httpClient.PostAsync(requestUri, content);
        }

        public static async Task<T> GetBody<T>(this HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<T>(content, options);
        }
    }
}
