using System.Linq;
using Api.Models;

namespace Api.Services
{
    public class UsersService
    {
        private readonly ApplicationContext _db;

        public UsersService(ApplicationContext db)
        {
            _db = db;
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
    }
}