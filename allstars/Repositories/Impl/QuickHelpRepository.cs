using allstars.Contexts;
using allstars.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    internal class QuickHelpRepository : RepositoryBase<QuickHelp>, IQuickHelpRepository
    {
        public QuickHelpRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<QuickHelp> GetQuickHelpByTagAsync(string tag)
        {
            var tickets = await FindByConditionAsync(x => x.Tag.CompareTo(tag.ToLower()) == 0);
            return tickets.DefaultIfEmpty(new QuickHelp()).FirstOrDefault();
        }

        public async Task<IEnumerable<QuickHelp>> GetAllQuickHelps()
        {
            return await FindAllAsync();
        }

        public async Task AddQuickHelpAsync(QuickHelp quickHelp)
        {
            await CreateAsync(quickHelp);
        }

        public void UpdateQuickHelp(QuickHelp quickHelp)
        {
            Update(quickHelp);
        }

        public void RemoveQuickHelp(QuickHelp quickHelp)
        {
            Delete(quickHelp);
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