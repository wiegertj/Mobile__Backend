using Entities.Models;
using System;

namespace Contracts
{
    public interface IAppUserRepository : IRepositoryBase<User>
    {
        void RegisterUser(User user);
        void DeleteUser(User user);
        void ChangePassword(User user);
        bool ValidateUser(User user);
        bool CheckIfExisting(String email);
        User GetUserByEmail(String email);
        User GetUserById(long userId);
    }
}
