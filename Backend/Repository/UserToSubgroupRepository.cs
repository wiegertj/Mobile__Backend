using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class UserToSubgroupRepository : RepositoryBase<UserToSubgroup>, IUserToSubgroupRepository
    {
        public UserToSubgroupRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public void AddMembership(UserToSubgroup userToSubgroup)
        {
            Create(userToSubgroup);
        }

        public void DeleteMembership(UserToSubgroup userToSubgroup)
        {
            Delete(userToSubgroup);
        }

        public IEnumerable<User> GetMembersForSubgroup(Subgroup subgroup)
        {
            var userToSubgroupList = FindByCondition(uts => uts.SubgroupId == subgroup.Id).ToList();
            var userList = new List<User>();

            foreach (var uts in userToSubgroupList) {
                var currentUser = RepositoryContext.Users.Find(uts.UserId);
                userList.Add(currentUser);
            }

            return userList;
        }

        public IEnumerable<UserToSubgroup> GetMembershipsForSubgroup(Subgroup subgroup)
        {
            return FindByCondition(uts => uts.SubgroupId.Equals(subgroup.Id));
        }

        public IEnumerable<UserToSubgroup> GetMembershipsForUser(User user)
        {
            return FindByCondition(uts => uts.UserId.Equals(user.Id));
        }

        public IEnumerable<Subgroup> GetSubgroupsForUser(User user)
        {
            var userToSubgroupList = FindByCondition(uts => uts.SubgroupId.Equals(user.Id)).ToList();
            var subgroupList = new List<Subgroup>();

            foreach (var uts in userToSubgroupList) {
                var currentSubgroup = RepositoryContext.Subgroups.Find(uts.SubgroupId);
                subgroupList.Add(currentSubgroup);
            }

            return subgroupList;
        }
    }
}
