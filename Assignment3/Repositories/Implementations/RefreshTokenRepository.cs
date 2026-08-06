using Assignment3.Data;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Implementations
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly FoodOrderingContext _context;

        public RefreshTokenRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task DeleteRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);

            await _context.SaveChangesAsync();
        }
    }
}
