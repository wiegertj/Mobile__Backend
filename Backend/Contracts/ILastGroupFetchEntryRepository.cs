using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface ILastGroupFetchEntryRepository : IRepositoryBase<LastGroupFetch>
    {
        LastGroupFetch Get(long userId, long groupId);
        void PostOrUpdate(LastGroupFetch fetch);
    }
}
