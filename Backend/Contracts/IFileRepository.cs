using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface IDiscussionEntryRepository : IRepositoryBase<DiscussionEntry>
    {
        void PostDiscussion(DiscussionEntry entry);
        IEnumerable<DiscussionEntry> GetSubgroupDiscussionEntries(int id);
        IEnumerable<DiscussionEntry> GetGroupDiscussionEntries(int id);
        DiscussionEntry GetDiscussionEntryById(long id);
    }
}
