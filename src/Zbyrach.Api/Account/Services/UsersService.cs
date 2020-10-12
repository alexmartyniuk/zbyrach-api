using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

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

        public Task<User> FindByEmail(string email)
        {
            return _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> FindById(long userId)
        {
            return await _db.Users.FindAsync(userId);
        }

        public async Task<User> AddNew(User user)
        {
            if (user.Id != default)
            {
                throw new Exception("A new user should not have an Id.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new Exception("A new user should have not empty email.");
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public ValueTask<User> GetCurrent()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            return _db.Users.FindAsync(long.Parse(userId));
        }

        public Task<List<User>> GetManyByIds(List<long> userIds)
        {
            return _db
                .Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public string GetUnsubscribeTokenByUser(User user)
        {
            return Encrypt($"userId:{user.Id}");
        }

        public async Task<User> GetUserByUnsubscribeToken(string secret)
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

        private string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private string Decrypt(string cipherText)
        {
            try
            {
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}