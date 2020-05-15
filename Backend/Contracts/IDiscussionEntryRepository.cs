using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface IDiscussionEntryRepository : IRepositoryBase<DiscussionEntry>
    {
        void PostDiscussion(DiscussionEntry entry);
        IEnumerable<DiscussionEntryReturnType> GetSubgroupDiscussionEntries(long id, int? answertTo, int? skip, int? take);
        IEnumerable<DiscussionEntryReturnType> GetGroupDiscussionEntries(long id, int? answertTo, int? skip, int? take);
        DiscussionEntryReturnType GetDiscussionEntryById(long id);
    }
}
