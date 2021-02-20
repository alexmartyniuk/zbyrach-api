using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace Zbyrach.Api.Account
{
    public class UsersService
    {
        private const string ENCRYPTION_KEY = "6917937020";
        private const string ENCRYPTION_PREFIX = "userId:";
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersService(ApplicationContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> FindById(long userId)
        {
            return await _db.Users.FindAsync(userId);
        }

        public ValueTask<User> GetCurrent()
        {
            var userIdClaim = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return new ValueTask<User>(default(User)!);
            }

            return _db.Users.FindAsync(long.Parse(userIdClaim.Value));
        }

        public string GetUnsubscribeTokenByUser(User user)
        {
            return Encrypt($"userId:{user.Id}");
        }

        public async Task<User?> GetUserByUnsubscribeToken(string secret)
        {
            var clearText = Decrypt(secret);
            if (clearText == null || !clearText.StartsWith(ENCRYPTION_PREFIX))
            {
                return null;
            }

            clearText = clearText.Substring(ENCRYPTION_PREFIX.Length);
            if (!long.TryParse(clearText, out var userId))
            {
                return null;
            }

            return await _db.Users.FindAsync(userId);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _db
                .Users
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<string> GetLanguage(User? user = null)
        {
            user ??= await GetCurrent();

            var readUser = await FindById(user.Id);

            return readUser.Language;
        }

        public async Task SetLanguage(string language, User? user = null)
        {
            user ??= await GetCurrent();

            var readUser = await FindById(user.Id);

            readUser.Language = language;

            await _db.SaveChangesAsync();
        }

        private string Encrypt(string clearText)
        {
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using var encryptor = Aes.Create();
            var pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(clearBytes, 0, clearBytes.Length);
                cs.Close();
            }
            clearText = Convert.ToBase64String(ms.ToArray());

            return clearText;
        }

        private string? Decrypt(string cipherText)
        {
            try
            {
                cipherText = cipherText.Replace(" ", "+");
                var cipherBytes = Convert.FromBase64String(cipherText);
                using var encryptor = Aes.Create();
                var pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());

                return cipherText;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}