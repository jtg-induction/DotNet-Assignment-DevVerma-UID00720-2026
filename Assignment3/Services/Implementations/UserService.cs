using System;
using System.Threading.Tasks;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using BCrypt.Net;
using Assignment3.Enums;

namespace Assignment3.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public UserService(
            IUserRepository userRepository, 
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository
            )
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<ApiResponse<object>> SignUpAsync(SignUpRequestDto request)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email already exists.",
                    Data = null
                };
            }

            User user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.customer.ToString(),
                Balance = 1000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _userRepository.AddUser(user);
            await _userRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "User created successfully.",
                Data = null
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
                return null;

            bool isPasswordCorrect =
                BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!isPasswordCorrect)
                return null;

            string accessToken =
                _jwtService.GenerateAccessToken(user);

            string refreshToken =
                _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.SaveRefreshTokenAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow
                });

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<bool> LogoutAsync(LogoutRequestDto request)
        {
            var refreshToken = await _refreshTokenRepository
                .GetRefreshTokenAsync(request.RefreshToken);

            if (refreshToken == null)
                return false;

            await _refreshTokenRepository
                .DeleteRefreshTokenAsync(refreshToken);

            return true;
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var refreshToken = await _refreshTokenRepository
                .GetRefreshTokenAsync(request.RefreshToken);

            if (refreshToken == null)
                return null;

            if (refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                await _refreshTokenRepository.DeleteRefreshTokenAsync(refreshToken);
                return null;
            }

            var user = refreshToken.User;

            string newAccessToken = _jwtService.GenerateAccessToken(user);

            string newRefreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.DeleteRefreshTokenAsync(refreshToken);

            await _refreshTokenRepository.SaveRefreshTokenAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow
                });

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<ApiResponse<object>> DeactivateUserAsync(string accessToken)
        {
            long userId = _jwtService.GetUserIdFromToken(accessToken);

            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (!user.IsActive)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User is already deactivated."
                };
            }

            await _refreshTokenRepository
                .DeleteAllRefreshTokensByUserIdAsync(userId);

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Account deactivated successfully.",
                Data = null
            };
        }

        public async Task<ApiResponse<object>> UpdatePasswordAsync(UpdatePasswordRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                };
            }

            bool isPasswordCorrect =
                BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password);

            if (!isPasswordCorrect)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Current password is incorrect.",
                    Data = null
                };
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Password updated successfully.",
                Data = null
            };
        }
    }
}
