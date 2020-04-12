using Entities.Models;
using System.Collections.Generic;

namespace Contracts
{
    public interface IGroupRepository : IRepositoryBase<Group>
    {
        IEnumerable<Group> GetPublicGroups();
        Group GetGroupById(long id);
        void CreateGroup(Group group);
        void DeleteGroup(Group group);
    }
}
