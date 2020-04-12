using Entities.Models;

namespace Entities.Extensions
{
    public static class GroupExtensions
    {
        public static void Map(this Group dbGroup, Group group) {
            dbGroup.Description = group.Description;
            dbGroup.Name = group.Name;
            dbGroup.IsPublic = group.IsPublic;
        }
    }
}
