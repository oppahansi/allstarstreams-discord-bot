using allstars.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface IUpcomingReleaseRepository
    {
        Task AddUpcomingReleaseAsync(UpcomingRelease upcomingRelease);

        Task AddManyUpcomingReleasesAsync(IEnumerable<UpcomingRelease> upcomingReleases);

        Task<IEnumerable<UpcomingRelease>> GetUpcomingReleases(DateTime releaseDate, bool posted = false);

        Task<IEnumerable<UpcomingRelease>> GetUpcomingReleases(int month, bool posted = false);
        Task<IEnumerable<UpcomingRelease>> GetAllUpcomingReleases();

        Task<IEnumerable<UpcomingRelease>> GetPastReleasesAsync();

        void UpdateUpcomingRelease(UpcomingRelease upcomingRelease);

        void DeleteUpcomingRelease(UpcomingRelease upcomingRelease);

        void DeleteManyUpcomingRelease(IEnumerable<UpcomingRelease> upcomingReleases);

        void SaveChanges();

        Task SaveChangesAsync();
    }
}