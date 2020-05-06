using Entities.Models;

namespace Entities.Extensions
{
    public static class UserExtensions
    {
        public static void Map(this User dbUser, User user) {
            dbUser.UserName = user.UserName;
            dbUser.FirstName = user.FirstName;
            dbUser.LastName = user.LastName;
            dbUser.Course_of_Studies = user.Course_of_Studies;
        }

        public static bool ValidateRegisterUser(this User user)
        {
            if ((user.UserName == null) || user.UserName.Equals(""))
            {
                return false;
            }

            if ((user.FirstName == null) || user.FirstName.Equals(""))
            {
                return false;
            }

            if ((user.LastName == null) || user.LastName.Equals(""))
            {
                return false;
            }

            if ((user.Course_of_Studies == null) || user.Course_of_Studies.Equals(""))
            {
                return false;
            }

            if ((user.Email == null) || user.Email.Equals(""))
            {
                return false;
            }

            if (user.University_Id.Equals(""))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateChangeUser(this User user)
        {
            if ((user.UserName == null) || user.UserName.Equals(""))
            {
                return false;
            }

            if ((user.FirstName == null) || user.FirstName.Equals(""))
            {
                return false;
            }

            if ((user.LastName == null) || user.LastName.Equals(""))
            {
                return false;
            }

            if ((user.Course_of_Studies == null) || user.Course_of_Studies.Equals(""))
            {
                return false;
            }

            return true;
        }
    }
}
