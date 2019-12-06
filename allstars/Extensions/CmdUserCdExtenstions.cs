using allstars.Models;

namespace allstars.Extensions
{
    public static class CmdUserCdExtenstions
    {
        public static bool IsEmpty(this CmdUserCd cmdCd)
        {
            return string.IsNullOrEmpty(cmdCd.Command) && cmdCd.Guild == 0 && cmdCd.UserId == 0;
        }

        public static bool IsObjectNull(this CmdUserCd cmdCd)
        {
            return cmdCd == null;
        }
    }
}