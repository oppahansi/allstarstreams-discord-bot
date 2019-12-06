using allstars.Models;

namespace allstars.Extensions
{
    public static class CmdChannelExtenstions
    {
        public static bool IsEmpty(this CmdChannel cmd)
        {
            return string.IsNullOrEmpty(cmd.Command) && cmd.Guild == 0 && cmd.ChannelId == 0;
        }

        public static bool IsObjectNull(this CmdChannel cmd)
        {
            return cmd == null;
        }
    }
}