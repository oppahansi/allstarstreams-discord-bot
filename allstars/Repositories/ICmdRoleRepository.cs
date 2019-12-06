using allstars.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface ICmdRoleRepository
    {
        Task AddCmdRoleAsync(CmdRole cmd);

        Task<CmdRole> GetCmdRoleAsync(string cmd);

        Task<IEnumerable<CmdRole>> GetAllCmdRolesAsync();

        void UpdateCmdRole(CmdRole cmd);

        void DeleteCmdRole(CmdRole cmd);

        void SaveChanges();
    }
}