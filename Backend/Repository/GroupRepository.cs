using Contracts;
using Entities;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repository
{
    public class GroupRepository : RepositoryBase<Group>, IGroupRepository
    {
        public GroupRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public void CreateGroup(Group group)
        {
            Create(group);
        }

        public void DeleteGroup(Group group)
        {
            Delete(group);
        }

        public Group GetGroupById(long id)
        {
            return FindByCondition(gr => gr.Id == id).FirstOrDefault();
        }

        public IEnumerable<Group> GetPublicGroups()
        {
            return FindByCondition(gr => gr.IsPublic.Equals("true")).ToList();
        }
    }
}
