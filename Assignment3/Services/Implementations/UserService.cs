using System;
using System.Threading.Tasks;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using BCrypt.Net;

namespace Assignment3.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<object>> SignUpAsync(SignUpRequestDto request)
        {
            try
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
                    Role = "customer",
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
            catch (Exception ex) {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
