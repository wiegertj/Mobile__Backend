using System;
using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class DiscussionEntryRepository : RepositoryBase<DiscussionEntry>, IDiscussionEntryRepository
    {
        public DiscussionEntryRepository(RepositoryContext context) : base(context) { }

        public DiscussionEntry GetDiscussionEntryById(long id)
        {
            return FindByCondition(uts => uts.Id.Equals(id)).First();
        }

        public IEnumerable<DiscussionEntry> GetGroupDiscussionEntries(int id, DateTime? since)
        {
            if (!since.HasValue)
            {
                return FindByCondition(uts => uts.NormalGroup.Equals(id)).OrderBy(uts => uts.TimeStamp).ToList();
            }
            return FindByCondition(uts => uts.NormalGroup.Equals(id) && uts.TimeStamp >= since.Value).OrderBy(uts => uts.TimeStamp).ToList();
        }

        public IEnumerable<DiscussionEntry> GetSubgroupDiscussionEntries(int id, DateTime? since)
        {
            if (!since.HasValue)
            {
                return FindByCondition(uts => uts.Subgroup.Equals(id)).OrderBy(uts => uts.TimeStamp).ToList();
            }
            return FindByCondition(uts => uts.Subgroup.Equals(id) && uts.TimeStamp >= since.Value).OrderBy(uts => uts.TimeStamp).ToList();
        }

        public void PostDiscussion(DiscussionEntry entry)
        {
            Create(entry);
        }
    }
}
