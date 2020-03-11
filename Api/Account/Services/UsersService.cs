using System.Linq;
using System.Security.Claims;
using MediumGrabber.Api.Migrations;
using Microsoft.AspNetCore.Http;

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

        public User GetUserByEmail(string email)
        {
            return _db.Users.SingleOrDefault(u => u.Email == email);
        }

        public User AddNewUser(User user)
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
            _db.SaveChanges();

            return user;
        }

        public User GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            return _db.Users.Find(long.Parse(userId));
        }
    }
}