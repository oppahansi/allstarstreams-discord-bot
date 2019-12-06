using allstars.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface ICmdChannelRepository
    {
        Task AddCmdChannelAsync(CmdChannel cmd);

        Task<CmdChannel> GetCmdChannelAsync(string cmd, ulong channelId);

        Task<IEnumerable<CmdChannel>> GetCmdChannelsAsync(string cmd);

        Task<IEnumerable<CmdChannel>> GetAllCmdChannelsAsync();

        void UpdateCmdChannel(CmdChannel cmd);

        void DeleteCmdChannel(CmdChannel cmd);

        void DeleteManyCmdChannels(IEnumerable<CmdChannel> cmds);

        void SaveChanges();
    }
}