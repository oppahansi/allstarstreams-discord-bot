using allstars.Contexts;
using allstars.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    public class CmdUserCdRepository : RepositoryBase<CmdUserCd>, ICmdUserCdRepository
    {
        public CmdUserCdRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task AddCmdUserCdAsync(CmdUserCd cmdCd)
        {
            await CreateAsync(cmdCd);
        }

        public void DeleteCmdUserCd(CmdUserCd cmdCd)
        {
            Delete(cmdCd);
        }

        public void DeleteManyCmdUserCds(IEnumerable<CmdUserCd> cmdCds)
        {
            DeleteMany(cmdCds);
        }

        public async Task<IEnumerable<CmdUserCd>> GetAllCmdUserCds()
        {
            return await FindAllAsync();
        }

        public async Task<CmdUserCd> GetCmdUserCd(string command, ulong userId)
        {
            var tickets = await FindByConditionAsync(x => x.Command.CompareTo(command.ToLower()) == 0 && x.UserId == userId);
            return tickets.DefaultIfEmpty(new CmdUserCd()).FirstOrDefault();
        }

        public async Task<IEnumerable<CmdUserCd>> GetExpiredCmdUserCds()
        {
            return await FindByConditionAsync(x => DateTime.Compare(x.Expires, DateTime.UtcNow) <= 0);
        }

        public void SaveChanges()
        {
            Save();
        }

        public void UpdateCmdUserCd(CmdUserCd cmdCd)
        {
            Update(cmdCd);
        }
    }
}