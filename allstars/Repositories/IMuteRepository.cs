using allstars.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface IMuteRepository
    {
        Task<Mute> GetActiveMuteByUserIdAsync(ulong userId);

        Task<IEnumerable<Mute>> GetAllMutesForUserIdAsync(ulong userId);

        Task<IEnumerable<Mute>> GetAllActiveMutesAsync();

        Task AddMuteAsync(Mute mute);

        void UpdateMute(Mute mute);

        void SaveChanges();
    }
}