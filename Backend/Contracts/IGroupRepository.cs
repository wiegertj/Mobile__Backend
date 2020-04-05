using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
