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

        public static bool ValidateCreateGroup(this Group group)
        {
            if ((group.Name == null) || group.Name.Equals(""))
            {
                return false;
            }

            if ((group.Description == null) || group.Description.Equals(""))
            {
                return false;
            }

            if (!(group.IsPublic.Equals("true") || group.IsPublic.Equals("false")))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateChangeGroup(this Group group)
        {
            if ((group.Name == null) || group.Name.Equals(""))
            {
                return false;
            }

            if ((group.Description == null) || group.Description.Equals(""))
            {
                return false;
            }

            if (!(group.IsPublic.Equals("true") || group.IsPublic.Equals("false")))
            {
                return false;
            }

            if (group.Id.Equals(""))
            {
                return false;
            }

            return true;
        }
    }
}
