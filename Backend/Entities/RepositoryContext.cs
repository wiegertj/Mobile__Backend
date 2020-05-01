using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserToGroup> UserToGroups { get; set; }
        public DbSet<Subgroup> Subgroups { get; set; }
        public DbSet<UserToSubgroup> UserToSubgroups { get; set; }
        public DbSet<DiscussionEntry> DiscussionEntries { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<LastGroupFetch> LastGroupFetches { get; set; }
        public DbSet<LastSubGroupFetch> LastSubGroupFetches { get; set; }
    }
}
