using System;
using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class LastGroupFetchEntryRepository : RepositoryBase<LastGroupFetch>, ILastGroupFetchEntryRepository
    {
        public LastGroupFetchEntryRepository(RepositoryContext context) : base(context) { }

        public LastGroupFetch Get(long userId, long groupId)
        {
            return FindByCondition(uts => uts.UserId == userId && uts.GroupId == groupId).First();
        }

        public void PostOrUpdate(LastGroupFetch fetch)
        {
            var entry = RepositoryContext.Entry(fetch);

            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added || entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                Update(fetch);
            } else
            {
                Create(fetch);
            }
        }
    }
}
