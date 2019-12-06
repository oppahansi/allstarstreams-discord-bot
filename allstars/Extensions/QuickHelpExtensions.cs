using allstars.Models;

namespace allstars.Extensions
{
    public static class QuickHelpExtensions
    {
        public static bool IsEmpty(this QuickHelp quickHelp)
        {
            return string.IsNullOrEmpty(quickHelp.Tag) && string.IsNullOrEmpty(quickHelp.Help);
        }

        public static bool IsObjectNull(this QuickHelp quickHelp)
        {
            return quickHelp == null;
        }
    }
}