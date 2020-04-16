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

        public IEnumerable<DiscussionEntry> GetGroupDiscussionEntries(int id)
        {
            return FindByCondition(uts => uts.NormalGroup.Equals(id));
        }

        public IEnumerable<DiscussionEntry> GetSubgroupDiscussionEntries(int id)
        {
            return FindByCondition(uts => uts.Subgroup.Equals(id));
        }

        public void PostDiscussion(DiscussionEntry entry)
        {
            Create(entry);
        }
    }
}
