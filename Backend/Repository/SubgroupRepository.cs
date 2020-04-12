using Contracts;
using Entities;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class SubgroupRepository : RepositoryBase<Subgroup>, ISubgroupRepository
    {
        public SubgroupRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public void CreateGroup(Subgroup subgroup)
        {
            Create(subgroup);
        }

        public void DeleteGroup(Subgroup subgroup)
        {
            Delete(subgroup);
        }

        public Subgroup GetSubgroupById(long subgroupId)
        {
            return FindByCondition(sgrp => sgrp.Id.Equals(subgroupId)).FirstOrDefault();
        }

        public IEnumerable<Subgroup> GetSubgroupsForGroup(long groupId)
        {
            return FindByCondition(sgrp => sgrp.Main_group.Equals(groupId));
        }
    }
}
