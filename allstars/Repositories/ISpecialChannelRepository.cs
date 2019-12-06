using allstars.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface ISpecialChannelRepository
    {
        Task<SpecialChannel> GetSpecialChannelByTypeAsync(string type);

        Task<IEnumerable<SpecialChannel>> GetAllSpecialChannelsAsync();

        Task AddSpecialChannelAsync(SpecialChannel specialChannel);

        void UpdateSpecialChannel(SpecialChannel specialChannel);

        void RemoveSpecialChannel(SpecialChannel specialChannel);

        Task SaveChangesAsync();

        void SaveChanges();
    }
}