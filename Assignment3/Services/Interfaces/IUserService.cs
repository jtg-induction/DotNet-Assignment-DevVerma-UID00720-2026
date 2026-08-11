using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<object>> SignUpAsync(SignUpRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<bool> LogoutAsync(string refreshToken);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse<object>> DeactivateUserAsync(long userId);
        Task<ApiResponse<object>> UpdatePasswordAsync(UpdatePasswordRequestDto request);

    }
}
