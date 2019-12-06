using allstars.Contexts;
using allstars.Models;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    internal class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task AddUserAsync(User user)
        {
            await CreateAsync(user);
        }

        public void AddUser(User user)
        {
            Create(user);
        }

        public async Task<User> GetUserByIdAsync(ulong userId)
        {
            var users = await FindByConditionAsync(x => x.Id == userId);
            return users.DefaultIfEmpty(new User()).FirstOrDefault();
        }

        public void SaveChanges()
        {
            Save();
        }

        public void UpdateUser(User user)
        {
            Update(user);
        }
    }
}