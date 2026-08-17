using Assignment3.Models;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        void AddUser(User user);
        Task<User> GetUserByIdAsync(long id);
        Task SaveAsync();
    }
}
