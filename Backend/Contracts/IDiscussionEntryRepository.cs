using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface IDiscussionEntryRepository : IRepositoryBase<DiscussionEntry>
    {
        void PostDiscussion(DiscussionEntry entry);
        IEnumerable<DiscussionEntryReturnType> GetSubgroupDiscussionEntries(long id, DateTime? since);
        IEnumerable<DiscussionEntryReturnType> GetGroupDiscussionEntries(long id, DateTime? since);
        DiscussionEntryReturnType GetDiscussionEntryById(long id);
    }
}
