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

        public static bool ValidateCreateSubgroup(this Subgroup subgroup)
        {
            if ((subgroup.Name == null) || subgroup.Name.Equals(""))
            {
                return false;
            }

            if ((subgroup.Description == null) || subgroup.Description.Equals(""))
            {
                return false;
            }

            if (subgroup.Main_group.Equals(""))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateUpdateSubgroup(this Subgroup subgroup)
        {
            if ((subgroup.Name == null) || subgroup.Name.Equals(""))
            {
                return false;
            }

            if ((subgroup.Description == null) || subgroup.Description.Equals(""))
            {
                return false;
            }

            if (subgroup.Main_group.Equals(""))
            {
                return false;
            }

            if (subgroup.Id.Equals(""))
            {
                return false;
            }

            return true;
        }
    }
}
