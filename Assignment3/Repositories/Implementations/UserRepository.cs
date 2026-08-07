using System.Data.Entity;
using System.Threading.Tasks;
using Assignment3.Data;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;

namespace Assignment3.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly FoodOrderingContext _context;

        public UserRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public async Task<User> GetUserByIdAsync(long userId)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
