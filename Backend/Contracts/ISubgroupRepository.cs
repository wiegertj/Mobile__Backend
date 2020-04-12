using Entities.Models;
using System;
using System.Collections.Generic;
namespace Contracts
{
    public interface ISubgroupRepository : IRepositoryBase<Subgroup>
    {
        IEnumerable<Subgroup> GetSubgroupsForGroup(long groupId);
        void CreateGroup(Subgroup subgroup);
        void DeleteGroup(Subgroup subgroup);
        Subgroup GetSubgroupById(long subgroupId);
    }
}
