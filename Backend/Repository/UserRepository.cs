using Contracts;
using Entities;
using Entities.Models;
using System;
using System.Linq;

namespace Repository
{
    public class UserRepository : RepositoryBase<User>, IAppUserRepository
    {
        public UserRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
        public void ChangePassword(User user)
        {
            var userDb = FindByCondition(us => us.Email.Equals(user.Email))
                .FirstOrDefault();

            if(userDb != null)
            {
                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                userDb.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, salt);

                Update(userDb);
            }
        }

        public void DeleteUser(User user)
        {
            Delete(user);
        }

        public void RegisterUser(User user)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, salt);

            Create(user);

        }

        public bool ValidateUser(User user)
        {
            var userDb = FindByCondition(us => us.Email.Equals(user.Email))
                .FirstOrDefault();

            if (userDb == null)
            {
                return false;
            }
            else if (BCrypt.Net.BCrypt.Verify(user.Password, userDb.Password))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool CheckIfExisting(String username)
        {
            var userDb = FindByCondition(us => us.Email.Equals(username))
                .FirstOrDefault();

            if(userDb == null)
            {
                return false;
            }

            return true;
        }

        public User GetUserByEmail(String username)
        {
            var userDb = FindByCondition(us => us.Email.Equals(username))
                .FirstOrDefault();

            if (userDb == null)
            {
                return null;
            }

            return userDb;
        }

        public User GetUserById(long userId)
        {
            var userDb = FindByCondition(us => us.Id.Equals(userId))
                .FirstOrDefault();

            if (userDb == null)
            {
                return null;
            }

            return userDb;
        }
    }
}
