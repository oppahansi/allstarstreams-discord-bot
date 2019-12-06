using allstars.Models;

namespace allstars.Extensions
{
    public static class UserExtensions
    {
        public static bool IsEmpty(this User user)
        {
            return user.Id == 0 && user.Guild == 0;
        }

        public static bool IsObjectNull(this User user)
        {
            return user == null;
        }
    }
}