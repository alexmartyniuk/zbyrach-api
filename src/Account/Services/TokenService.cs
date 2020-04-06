using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;

namespace Zbyrach.Api.Account
{
    public class TokenService
    {
        private readonly ApplicationContext _db;
        public HttpClient _http { get; set; }

        public TokenService(ApplicationContext db)
        {
            _db = db;
            _http = new HttpClient();
        }

        public async Task<AccessToken> GetTokenWithUserByValue(string token)
        {
            var accessToken = await _db.AccessTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);

            if (accessToken != null && accessToken.ExpiredAt > DateTime.UtcNow)
            {
                return accessToken;
            }

            return null;
        }

        public Task<AccessToken> GetTokenByUser(User user)
        {
            return _db.AccessTokens
                .SingleOrDefaultAsync(t => t.UserId == user.Id);
        }

        public async Task<bool> RemoveToken(AccessToken token)
        {
            var existingToken = await _db.AccessTokens.FindAsync(token.Id);
            if (existingToken == null)
            {
                return false;
            }

            _db.AccessTokens.Remove(existingToken);
            return await _db.SaveChangesAsync() > 0;
        }


        public async Task<GoogleToken> ValidateGoogleToken(string idToken)
        {
            try
            {
                var response = await _http.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                return JsonSerializer.Deserialize<GoogleToken>(response);
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public AccessToken CreateFromGoogleToken(GoogleToken googleToken, string authToken)
        {
            return new AccessToken
            {
                ExpiredAt = UnixTimestampToDateTime(googleToken.exp),
                Token = authToken
            };
        }

        private DateTime UnixTimestampToDateTime(string unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddSeconds(int.Parse(unixTimeStamp)).ToUniversalTime();
        }

        public async Task<AccessToken> SaveToken(User user, AccessToken validToken)
        {
            var existingToken = await _db.AccessTokens
                .SingleOrDefaultAsync(t => t.UserId == user.Id);
            if (existingToken != null)
            {
                _db.Remove(existingToken);
            }

            validToken.User = user;
            _db.AccessTokens.Add(validToken);
            await _db.SaveChangesAsync();

            return validToken;
        }
    }
}