using allstars.Contexts;
using allstars.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    internal class MuteRepository : RepositoryBase<Mute>, IMuteRepository
    {
        public MuteRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<Mute> GetActiveMuteByUserIdAsync(ulong userId)
        {
            var mutes = await FindByConditionAsync(x => x.UserId == userId && x.Active);
            return mutes.DefaultIfEmpty(new Mute()).FirstOrDefault();
        }

        public async Task<IEnumerable<Mute>> GetAllActiveMutesAsync()
        {
            return await FindByConditionAsync(x => x.Active == true);
        }

        public async Task<IEnumerable<Mute>> GetAllMutesForUserIdAsync(ulong userId)
        {
            return await FindByConditionAsync(x => x.UserId == userId);
        }

        public async Task AddMuteAsync(Mute mute)
        {
            await CreateAsync(mute);
        }

        public void SaveChanges()
        {
            Save();
        }

        public void UpdateMute(Mute mute)
        {
            Update(mute);
        }
    }
}