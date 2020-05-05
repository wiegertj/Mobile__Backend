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

        public DiscussionEntryReturnType GetDiscussionEntryById(long id)
        {
            return GetUserJoined(uts => uts.Id.Equals(id)).First();
        }

        public IEnumerable<DiscussionEntryReturnType> GetGroupDiscussionEntries(long id, DateTime? since)
        {
            if (!since.HasValue)
            {
                return GetUserJoined(uts => uts.NormalGroup.Equals(id));
            }
            return GetUserJoined(uts => uts.NormalGroup.Equals(id) && uts.TimeStamp >= since.Value);
        }

        public IEnumerable<DiscussionEntryReturnType> GetSubgroupDiscussionEntries(long id, DateTime? since)
        {
            if (!since.HasValue)
            {
                return GetUserJoined(uts => uts.Subgroup.Equals(id));
            }
            return GetUserJoined(uts => uts.Subgroup.Equals(id) && uts.TimeStamp >= since.Value);
        }

        private IList<DiscussionEntryReturnType> GetUserJoined(System.Linq.Expressions.Expression<Func<DiscussionEntry, bool>> expression)
        {
            return RepositoryContext.Set<DiscussionEntry>().Where(expression).Join(RepositoryContext.Users, 
                discussionEntry => discussionEntry.UserId,
                user => user.Id,
                (discussionEntry, user) => new DiscussionEntryReturnType()
                    {
                        discussionEntry = discussionEntry,
                        UserName = user.UserName
                    }
                ).OrderBy(uts => uts.discussionEntry.TimeStamp).ToList();
        }

        public void PostDiscussion(DiscussionEntry entry)
        {
            Create(entry);
        }
    }
}
