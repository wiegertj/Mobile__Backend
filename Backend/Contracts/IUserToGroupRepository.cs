using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IUserToGroupRepository : IRepositoryBase<UserToGroup>
    {
        void DeleteMembership(UserToGroup userToGroup);
        void AddMembership(UserToGroup userToGroup);
        IEnumerable<User> GetMembersForGroup(Group group);
        IEnumerable<Group> GetGroupsForUser(User user);
        IEnumerable<UserToGroup> GetMembershipsForGroup(Group group);
        IEnumerable<UserToGroup> GetMembershipsForUser(User user);
    }
}
