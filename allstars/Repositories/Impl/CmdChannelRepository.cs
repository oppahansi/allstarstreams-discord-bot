using allstars.Contexts;
using allstars.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    public class CmdChannelRepository : RepositoryBase<CmdChannel>, ICmdChannelRepository
    {
        public CmdChannelRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task AddCmdChannelAsync(CmdChannel cmd)
        {
            await CreateAsync(cmd);
        }

        public async Task<CmdChannel> GetCmdChannelAsync(string cmd, ulong channelId)
        {
            var cmdChannels = await FindByConditionAsync(x => x.Command.CompareTo(cmd.ToLower()) == 0 && x.ChannelId == channelId);
            return cmdChannels.DefaultIfEmpty(new CmdChannel()).FirstOrDefault();
        }

        public async Task<IEnumerable<CmdChannel>> GetCmdChannelsAsync(string cmd)
        {
            return await FindByConditionAsync(x => x.Command.CompareTo(cmd.ToLower()) == 0);
        }

        public async Task<IEnumerable<CmdChannel>> GetAllCmdChannelsAsync()
        {
            return await FindAllAsync();
        }

        public void UpdateCmdChannel(CmdChannel cmd)
        {
            Update(cmd);
        }

        public void DeleteCmdChannel(CmdChannel cmd)
        {
            Delete(cmd);
        }

        public void DeleteManyCmdChannels(IEnumerable<CmdChannel> cmds)
        {
            DeleteMany(cmds);
        }

        public void SaveChanges()
        {
            Save();
        }
    }
}