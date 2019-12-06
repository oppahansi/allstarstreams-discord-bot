using allstars.Contexts;
using allstars.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    internal class SpecialChannelRepository : RepositoryBase<SpecialChannel>, ISpecialChannelRepository
    {
        public SpecialChannelRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<SpecialChannel> GetSpecialChannelByTypeAsync(string type)
        {
            var tickets = await FindByConditionAsync(x => x.Type.CompareTo(type.ToLower()) == 0);
            return tickets.DefaultIfEmpty(new SpecialChannel()).FirstOrDefault();
        }

        public async Task<IEnumerable<SpecialChannel>> GetAllSpecialChannelsAsync()
        {
            return await FindAllAsync();
        }

        public async Task AddSpecialChannelAsync(SpecialChannel specialChannel)
        {
            await CreateAsync(specialChannel);
        }

        public void UpdateSpecialChannel(SpecialChannel specialChannel)
        {
            Update(specialChannel);
        }

        public void RemoveSpecialChannel(SpecialChannel specialChannel)
        {
            Delete(specialChannel);
        }

        public async Task SaveChangesAsync()
        {
            await SaveAsync();
        }

        public void SaveChanges()
        {
            Save();
        }
    }
}