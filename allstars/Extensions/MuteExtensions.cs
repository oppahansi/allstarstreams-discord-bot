using allstars.Models;

namespace allstars.Extensions
{
    public static class MuteExtensions
    {
        public static bool IsEmpty(this Mute mute)
        {
            return mute.UserId == 0;
        }

        public static bool IsObjectNull(this Mute mute)
        {
            return mute == null;
        }
    }
}