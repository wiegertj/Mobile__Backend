using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface ILastSubGroupFetchEntryRepository : IRepositoryBase<LastSubGroupFetch>
    {
        LastSubGroupFetch Get(long userId, long subGroupId);
        void PostOrUpdate(LastSubGroupFetch fetch);
    }
}
