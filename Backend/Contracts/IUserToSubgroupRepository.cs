using Entities.Models;
using System.Collections.Generic;

namespace Contracts
{
    public interface IUserToSubgroupRepository : IRepositoryBase<UserToSubgroup>
    {
        void DeleteMembership(UserToSubgroup userToSubgroup);
        void AddMembership(UserToSubgroup userToSubgroup);
        IEnumerable<User> GetMembersForSubgroup(Subgroup subgroup);
        IEnumerable<Subgroup> GetSubgroupsForUser(User user);
        IEnumerable<UserToSubgroup> GetMembershipsForSubgroup(Subgroup subgroup);
        IEnumerable<UserToSubgroup> GetMembershipsForUser(User user);
        bool IsMember(long userId, long groupId);
    }
}
