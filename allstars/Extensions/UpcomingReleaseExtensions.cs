using allstars.Models;
using System;

namespace allstars.Extensions
{
    public static class UpcomingReleaseExtensions
    {
        public static bool IsEmpty(this UpcomingRelease upcomingRelease)
        {
            return string.IsNullOrEmpty(upcomingRelease.Title) &&
            upcomingRelease.DvdReleaseDate == new DateTime() &&
            upcomingRelease.BluRayReleaseDate == new DateTime() &&
            upcomingRelease.UhdReleaseDate == new DateTime();
        }

        public static bool IsObjectNull(this UpcomingRelease upcomingRelease)
        {
            return upcomingRelease == null;
        }

        public static bool IsAlreadyPosted(this UpcomingRelease upcomingRelease)
        {
            return upcomingRelease.DvdReleasePosted && upcomingRelease.BluRayPosted && upcomingRelease.UhdPosted;
        }

        public static bool IsIdentical(this UpcomingRelease upcomingRelease, UpcomingRelease other)
        {
            return upcomingRelease.Title.CompareTo(other.Title) == 0 &&
                upcomingRelease.DvdReleaseDate.CompareTo(other.DvdReleaseDate) == 0 &&
                upcomingRelease.BluRayReleaseDate.CompareTo(other.BluRayReleaseDate) == 0 &&
                upcomingRelease.UhdReleaseDate.CompareTo(other.UhdReleaseDate) == 0;
        }
    }
}