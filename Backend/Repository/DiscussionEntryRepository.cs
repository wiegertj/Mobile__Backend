using System;
using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Repository
{
    public class DiscussionEntryRepository : RepositoryBase<DiscussionEntry>, IDiscussionEntryRepository
    {
        public DiscussionEntryRepository(RepositoryContext context) : base(context) { }

        public DiscussionEntryReturnType GetDiscussionEntryById(long id)
        {
            return GetUserJoined(uts => uts.Id.Equals(id)).First();
        }

        public IEnumerable<DiscussionEntryReturnType> GetGroupDiscussionEntries(long id, int? skip, int? take)
        {
            return GetUserJoined(uts => uts.NormalGroup.Equals(id), query => SkipAndTake(query, skip, take));
        }

        public IEnumerable<DiscussionEntryReturnType> GetSubgroupDiscussionEntries(long id, int? skip, int? take)
        {
            return GetUserJoined(uts => uts.Subgroup.Equals(id), query => SkipAndTake(query, skip, take));
        }

        private IList<DiscussionEntryReturnType> GetUserJoined(Expression<Func<DiscussionEntry, bool>> expression, Func<IQueryable<DiscussionEntryReturnType>, IQueryable<DiscussionEntryReturnType>> fun=null)
        {
            IQueryable<DiscussionEntryReturnType> v = RepositoryContext.Set<DiscussionEntry>().Where(expression).Join(RepositoryContext.Users,
                discussionEntry => discussionEntry.UserId,
                user => user.Id,
                (discussionEntry, user) => new DiscussionEntryReturnType()
                {
                    discussionEntry = discussionEntry,
                    UserName = user.UserName
                }).OrderBy(uts => uts.discussionEntry.TimeStamp);
            if (fun != null)
                v = fun(v);
            return v.ToList();
        }

        private IQueryable<DiscussionEntryReturnType> SkipAndTake(IQueryable<DiscussionEntryReturnType> query, int? skip, int? take)
        {
            if (skip.HasValue)
                query = query.Skip(skip.Value);
            if (take.HasValue)
                query = query.Take(take.Value);

            return query;
        }

        public void PostDiscussion(DiscussionEntry entry)
        {
            Create(entry);
        }
    }
}
