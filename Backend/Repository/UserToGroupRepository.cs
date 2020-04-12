using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class UserToGroupRepository : RepositoryBase<UserToGroup>, IUserToGroupRepository
    {
        public UserToGroupRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public void AddMembership(UserToGroup userToGroup)
        {
            Create(userToGroup);
        }

        public void DeleteMembership(UserToGroup userToGroup)
        {
            Delete(userToGroup);
        }

        public IEnumerable<Group> GetGroupsForUser(User user)
        {
            var userToGroupList = FindByCondition(utg => utg.UserId == user.Id).ToList();
            var groupList = new List<Group>();

            foreach (var utg in userToGroupList) {
                var currentGroup = RepositoryContext.Groups.Find(utg.GroupId);
                groupList.Add(currentGroup);
            }

            return groupList;
        }

        public IEnumerable<User> GetMembersForGroup(Group group)
        {
            var userToGroupList = FindByCondition(utg => utg.GroupId == group.Id).ToList();
            var userList = new List<User>();

            foreach (var utg in userToGroupList) {
                var currentUser = RepositoryContext.Users.Find(utg.UserId);
                userList.Add(currentUser);
            }

            return userList;
        }

        public IEnumerable<UserToGroup> GetMembershipsForUser(User user)
        {
            return FindByCondition(utg => utg.UserId == user.Id);
        }

        public IEnumerable<UserToGroup> GetMembershipsForGroup(Group group)
        {
            return FindByCondition(utg => utg.GroupId == group.Id);
        }
    }
}
