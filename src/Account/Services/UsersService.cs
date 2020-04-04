using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediumGrabber.Api.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MediumGrabber.Api.Account
{
    public class UsersService
    {
        private readonly ApplicationContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersService(ApplicationContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<User> GetUserByEmail(string email)
        {
            return _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public Task<List<User>> GetUsersWithTags()
        {
            return _db.Users
                .Include(u => u.TagUsers)
                .ThenInclude(tu => tu.Tag)
                .ToListAsync();
        }

        public async Task<User> AddNewUser(User user)
        {
            if (user.Id != default)
            {
                throw new System.Exception("A new user should not have an Id.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new System.Exception("A new user should have not empty email.");
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public ValueTask<User> GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            return _db.Users.FindAsync(long.Parse(userId));
        }
    }
}