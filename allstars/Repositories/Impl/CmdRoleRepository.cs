using allstars.Contexts;
using allstars.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    public class CmdRoleRepository : RepositoryBase<CmdRole>, ICmdRoleRepository
    {
        public CmdRoleRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task AddCmdRoleAsync(CmdRole cmd)
        {
            await CreateAsync(cmd);
        }

        public async Task<CmdRole> GetCmdRoleAsync(string cmd)
        {
            var cmdRoles = await FindByConditionAsync(x => x.Command.CompareTo(cmd.ToLower()) == 0);
            return cmdRoles.DefaultIfEmpty(new CmdRole()).FirstOrDefault();
        }

        public async Task<IEnumerable<CmdRole>> GetAllCmdRolesAsync()
        {
            return await FindAllAsync();
        }

        public void UpdateCmdRole(CmdRole cmd)
        {
            Update(cmd);
        }

        public void DeleteCmdRole(CmdRole cmd)
        {
            Delete(cmd);
        }

        public void SaveChanges()
        {
            Save();
        }
    }
}