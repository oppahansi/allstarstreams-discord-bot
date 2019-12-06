using allstars.Models;

namespace allstars.Extensions
{
    public static class CmdRoleExtenstions
    {
        public static bool IsEmpty(this CmdRole cmd)
        {
            return string.IsNullOrEmpty(cmd.Command) && cmd.Guild == 0 && cmd.MinRoleId == 0;
        }

        public static bool IsObjectNull(this CmdRole cmd)
        {
            return cmd == null;
        }
    }
}