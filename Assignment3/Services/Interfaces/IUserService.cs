using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<object>> SignUpAsync(SignUpRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<bool> LogoutAsync(LogoutRequestDto request);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<ApiResponse<object>> DeactivateUserAsync(string accessToken);
        Task<ApiResponse<object>> UpdatePasswordAsync(UpdatePasswordRequestDto request);
    }
}
