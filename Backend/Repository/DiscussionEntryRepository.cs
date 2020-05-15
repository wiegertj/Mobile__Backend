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

        public IEnumerable<DiscussionEntryReturnType> GetGroupDiscussionEntries(long id, int? answertTo, int? skip, int? take)
        {
            Expression<Func<DiscussionEntry, bool>> expr = null;
            if (answertTo.HasValue)
            {
                expr = uts => uts.NormalGroup.Equals(id) && uts.AnswerTo.HasValue && uts.AnswerTo.Value == answertTo.Value;
            }
            else
            {
                 expr = uts => uts.NormalGroup.Equals(id) && !uts.AnswerTo.HasValue;
            }
            return GetUserJoined(expr, query => SkipAndTake(query, skip, take));
        }

        public IEnumerable<DiscussionEntryReturnType> GetSubgroupDiscussionEntries(long id, int? answertTo, int? skip, int? take)
        {
            Expression<Func<DiscussionEntry, bool>> expr = null;
            if (answertTo.HasValue)
            {
                expr = uts => uts.Subgroup.Equals(id) && uts.AnswerTo.HasValue && uts.AnswerTo.Value == answertTo.Value;
            }
            else
            {
                expr = uts => uts.Subgroup.Equals(id) && !uts.AnswerTo.HasValue;
            }
            return GetUserJoined(expr, query => SkipAndTake(query, skip, take));
        }

        private IList<DiscussionEntryReturnType> GetUserJoined(Expression<Func<DiscussionEntry, bool>> expression, Func<IQueryable<DiscussionEntryReturnType>, IQueryable<DiscussionEntryReturnType>> fun=null)
        {
            IQueryable<DiscussionEntryReturnType> v = RepositoryContext.Set<DiscussionEntry>().Where(expression).Join(RepositoryContext.Users,
                discussionEntry => discussionEntry.UserId,
                user => user.Id,
                (discussionEntry, user) => new DiscussionEntryReturnType()
                {
                    Id = discussionEntry.Id,
                    Topic = discussionEntry.Topic,
                    Text = discussionEntry.Text,
                    TimeStamp = discussionEntry.TimeStamp,
                    Subgroup = discussionEntry.Subgroup,
                    NormalGroup = discussionEntry.NormalGroup,
                    UserId = discussionEntry.UserId,
                    AnswerTo = discussionEntry.AnswerTo,
                    File = discussionEntry.File,
                    UserName = user.UserName,
                    AnswerCount = RepositoryContext.DiscussionEntries.Count(uts => uts.AnswerTo == discussionEntry.Id)
                }).OrderBy(uts => uts.TimeStamp);
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
