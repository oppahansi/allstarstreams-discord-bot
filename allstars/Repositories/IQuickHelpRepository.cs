using allstars.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface IQuickHelpRepository
    {
        Task<QuickHelp> GetQuickHelpByTagAsync(string tag);

        Task<IEnumerable<QuickHelp>> GetAllQuickHelps();

        Task AddQuickHelpAsync(QuickHelp quickHelp);

        void UpdateQuickHelp(QuickHelp quickHelp);

        void RemoveQuickHelp(QuickHelp quickHelp);

        Task SaveChangesAsync();

        void SaveChanges();
    }
}