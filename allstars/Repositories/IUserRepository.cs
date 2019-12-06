using allstars.Models;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(ulong userId);

        Task AddUserAsync(User user);

        void AddUser(User user);

        void UpdateUser(User user);

        void SaveChanges();
    }
}