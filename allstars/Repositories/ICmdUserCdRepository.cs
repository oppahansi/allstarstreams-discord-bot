using allstars.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface ICmdUserCdRepository
    {
        Task AddCmdUserCdAsync(CmdUserCd cmdCd);

        Task<CmdUserCd> GetCmdUserCd(string command, ulong userId);

        Task<IEnumerable<CmdUserCd>> GetExpiredCmdUserCds();

        Task<IEnumerable<CmdUserCd>> GetAllCmdUserCds();

        void DeleteCmdUserCd(CmdUserCd cmdCd);

        void DeleteManyCmdUserCds(IEnumerable<CmdUserCd> cmdCd);

        void UpdateCmdUserCd(CmdUserCd cmdCd);

        void SaveChanges();
    }
}