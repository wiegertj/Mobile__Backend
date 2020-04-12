using Entities.Models;

namespace Entities.Extensions
{
    public static class SubgroupExtensions
    {
        public static void Map(this Subgroup dbSubgroup, Subgroup subgroup) {
            dbSubgroup.Description = subgroup.Description;
            dbSubgroup.Main_group = subgroup.Main_group;
            dbSubgroup.Name = subgroup.Name;
        }
    }
}
