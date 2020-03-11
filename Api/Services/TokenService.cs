using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Dtos;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
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

        public AccessToken GetTokenWithUser(string token)
        {
            var accessToken = _db.AccessTokens
                .Include(t => t.User)
                .SingleOrDefault(t => t.Token == token);

            if (accessToken != null && accessToken.ExpiredAt > DateTime.UtcNow)
            {
                return accessToken;
            }

            return null;
        }

        public GoogleToken ValidateGoogleToken(string idToken)
        {
            var response = _http.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}")
                .GetAwaiter()
                .GetResult();
            return JsonSerializer.Deserialize<GoogleToken>(response);
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

        public AccessToken SaveToken(User user, AccessToken validToken)
        {
            var existingToken = _db.AccessTokens.SingleOrDefault(t => t.UserId == user.Id);
            if (existingToken != null)
            {
                _db.Remove(existingToken);
            }

            validToken.User = user;
            _db.AccessTokens.Add(validToken);
            _db.SaveChanges();

            return validToken;
        }
    }
}