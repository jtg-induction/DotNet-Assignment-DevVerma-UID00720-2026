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

            if (user == null)
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

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var storedRefreshToken =
                await _refreshTokenRepository.GetRefreshTokenAsync(refreshToken);

            if (storedRefreshToken == null)
                return false;

            await _refreshTokenRepository
                .DeleteRefreshTokenAsync(storedRefreshToken);

            return true;
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshToken);

            if (storedRefreshToken == null)
                return null;

            if (storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                await _refreshTokenRepository.DeleteRefreshTokenAsync(storedRefreshToken);

                return null;
            }

            var user = storedRefreshToken.User;

            string newAccessToken =
                _jwtService.GenerateAccessToken(user);

            string newRefreshToken =
                _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository
                .DeleteRefreshTokenAsync(storedRefreshToken);

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
    }
}
