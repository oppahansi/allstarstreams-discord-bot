using allstars.Models;

namespace allstars.Extensions
{
    public static class SpecialChannelExtensions
    {
        public static bool IsEmpty(this SpecialChannel specialChannel)
        {
            return specialChannel.Id == 0 && specialChannel.Guild == 0 && string.IsNullOrEmpty(specialChannel.Type);
        }

        public static bool IsObjectNull(this SpecialChannel specialChannel)
        {
            return specialChannel == null;
        }
    }
}