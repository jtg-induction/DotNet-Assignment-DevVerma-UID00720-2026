using Assignment3.Models;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);

        Task<RefreshToken> GetRefreshTokenAsync(string token);

        Task DeleteRefreshTokenAsync(RefreshToken refreshToken);
        Task DeleteAllRefreshTokensByUserIdAsync(long userId);
    }
}
