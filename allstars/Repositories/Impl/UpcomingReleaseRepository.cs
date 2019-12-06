using allstars.Contexts;
using allstars.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    public class UpcomingReleaseRepository : RepositoryBase<UpcomingRelease>, IUpcomingReleaseRepository
    {
        public UpcomingReleaseRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task AddUpcomingReleaseAsync(UpcomingRelease upcomingRelease)
        {
            await CreateAsync(upcomingRelease);
        }

        public async Task AddManyUpcomingReleasesAsync(IEnumerable<UpcomingRelease> upcomingRelease)
        {
            await CreateManyAsync(upcomingRelease);
        }

        public void DeleteManyUpcomingRelease(IEnumerable<UpcomingRelease> upcomingReleases)
        {
            DeleteMany(upcomingReleases);
        }

        public void DeleteUpcomingRelease(UpcomingRelease upcomingRelease)
        {
            Delete(upcomingRelease);
        }

        public async Task<IEnumerable<UpcomingRelease>> GetUpcomingReleases(DateTime releaseDate, bool unpostedOnly = false)
        {
            if (!unpostedOnly)
            {
                return await FindByConditionAsync(x =>
                (x.DvdReleaseDate.CompareTo(releaseDate) == 0) ||
                (x.BluRayReleaseDate.CompareTo(releaseDate) == 0) ||
                (x.UhdReleaseDate.CompareTo(releaseDate) == 0));
            }
            return await FindByConditionAsync(x =>
                (x.DvdReleaseDate.CompareTo(releaseDate) == 0 && !x.DvdReleasePosted) ||
                (x.BluRayReleaseDate.CompareTo(releaseDate) == 0 && !x.BluRayPosted) ||
                (x.UhdReleaseDate.CompareTo(releaseDate) == 0 && !x.UhdPosted));
        }

        public async Task<IEnumerable<UpcomingRelease>> GetUpcomingReleases(int month, bool unpostedOnly = false)
        {
            if (!unpostedOnly)
            {
                return await FindByConditionAsync(x =>
                (x.DvdReleaseDate.Month == month ||
                x.BluRayReleaseDate.Month == month ||
                x.UhdReleaseDate.Month == month));
            }
            return await FindByConditionAsync(x =>
            (x.DvdReleaseDate.Month == month ||
            x.BluRayReleaseDate.Month == month ||
            x.UhdReleaseDate.Month == month) &&
            (!x.DvdReleasePosted || !x.BluRayPosted || !x.UhdPosted));
        }

        public async Task<IEnumerable<UpcomingRelease>> GetPastReleasesAsync()
        {
            return await FindByConditionAsync(x =>
            (x.DvdReleaseDate <= DateTime.Now.AddMonths(-1)) ||
            (x.BluRayReleaseDate <= DateTime.Now.AddMonths(-1)) ||
            (x.UhdReleaseDate <= DateTime.Now.AddMonths(-1)));
        }

        public async Task<IEnumerable<UpcomingRelease>> GetAllUpcomingReleases()
        {
            return await FindAllAsync();
        }

        public void SaveChanges()
        {
            Save();
        }

        public async Task SaveChangesAsync()
        {
            await SaveAsync();
        }

        public void UpdateUpcomingRelease(UpcomingRelease upcomingRelease)
        {
            Update(upcomingRelease);
        }
    }
}