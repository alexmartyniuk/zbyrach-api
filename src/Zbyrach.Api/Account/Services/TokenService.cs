using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Zbyrach.Api.Account
{
    public class TokenService
    {
        private readonly ApplicationContext _db;
        private readonly ILogger<TokenService> _logger;

        public HttpClient _http { get; set; }

        public TokenService(ApplicationContext db, ILogger<TokenService> logger)
        {
            _db = db;
            _logger = logger;
            _http = new HttpClient();
        }

        public async Task<AccessToken> GetTokenWithUser(string token)
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

        public Task<AccessToken> GetTokenByGoogleToken(string googleToken)
        {
            var token = GetTokenHash(googleToken);
            return GetTokenWithUser(token);
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
                _logger.LogError(e, "Google token could not be validated.");
                return null;
            }
        }

        public AccessToken CreateFromGoogleToken(GoogleToken googleToken, string authToken)
        {
            return new AccessToken
            {
                ExpiredAt = DateTime.UtcNow + TimeSpan.FromDays(30),
                Token = GetTokenHash(authToken)
            };
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

        public string GetTokenHash(string token)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var encoding = Encoding.ASCII;
            var data = encoding.GetBytes(token);

            Span<byte> hashBytes = stackalloc byte[16];
            md5.TryComputeHash(data, hashBytes, out int written);
            if (written != hashBytes.Length)
                throw new OverflowException();


            Span<char> stringBuffer = stackalloc char[32];
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashBytes[i].TryFormat(stringBuffer.Slice(2 * i), out _, "x2");
            }
            return new string(stringBuffer);
        }
    }
}