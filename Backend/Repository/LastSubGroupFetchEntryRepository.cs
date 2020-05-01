using System;
using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class LastSubGroupFetchEntryRepository : RepositoryBase<LastSubGroupFetch>, ILastSubGroupFetchEntryRepository
    {
        public LastSubGroupFetchEntryRepository(RepositoryContext context) : base(context) { }

        public LastSubGroupFetch Get(long userId, long subGroupId)
        {
            return FindByCondition(uts => uts.UserId == userId && uts.SubGroupId == subGroupId).First();
        }

        public void PostOrUpdate(LastSubGroupFetch fetch)
        {
            var entry = RepositoryContext.Entry(fetch);

            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added || entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                Update(fetch);
            }
            else
            {
                Create(fetch);
            }
        }
    }
}
