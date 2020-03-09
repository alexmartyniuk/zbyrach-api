using System;
using System.Linq;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class TokenService
    {
        private readonly ApplicationContext _db;

        public TokenService(ApplicationContext db)
        {
            _db = db;
        }

        public User GetUserByToken(string token)
        {
            var accessToken = _db.AccessTokens
                .Include(t => t.User)
                .SingleOrDefault(t => t.Token == token);

            if (accessToken.ExpiredAt > DateTime.UtcNow)
            {
                return accessToken.User;
            }

            return null;
        }
    }
}