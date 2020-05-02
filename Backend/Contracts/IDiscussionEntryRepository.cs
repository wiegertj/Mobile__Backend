using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface IDiscussionEntryRepository : IRepositoryBase<DiscussionEntry>
    {
        void PostDiscussion(DiscussionEntry entry);
        IEnumerable<DiscussionEntry> GetSubgroupDiscussionEntries(long id, DateTime? since);
        IEnumerable<DiscussionEntry> GetGroupDiscussionEntries(long id, DateTime? since);
        DiscussionEntry GetDiscussionEntryById(long id);
    }
}
