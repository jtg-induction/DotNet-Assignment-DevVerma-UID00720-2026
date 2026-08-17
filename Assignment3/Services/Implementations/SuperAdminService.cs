using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Enums;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Assignment3.Services.Implementations
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly FoodOrderingContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;

        public SuperAdminService(
            FoodOrderingContext context,
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
        }

        public async Task<ApiResponse<object>> AddRestaurantAsync(AddRestaurantRequestDto request)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var response =
                        await AddRestaurantInternalAsync(request);

                    if (!response.Success)
                    {
                        transaction.Rollback();
                        return response;
                    }

                    transaction.Commit();

                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<ApiResponse<object>> AddRestaurantInternalAsync(AddRestaurantRequestDto request)
        {
            if (request.RestaurantEmail.Equals(
                request.OwnerEmail,
                StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Restaurant email and owner email must be different.",
                    Data = null
                };
            }

            var existingRestaurant = await _restaurantRepository
                    .GetRestaurantByEmailAsync(request.RestaurantEmail);

            if (existingRestaurant != null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Restaurant with this email already exists.",
                    Data = null
                };
            }

            var existingUser = await _userRepository
                    .GetUserByEmailAsync(request.OwnerEmail);

            if (existingUser != null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User with this email already exists.",
                    Data = null
                };
            }

            var address = new Address
            {
                UserId = null,
                BuildingNumber = request.BuildingNumber,
                Locality = request.Locality,
                City = request.City,
                State = request.State,
                Country = request.Country,
                PostalCode = request.PostalCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _restaurantRepository.AddAddress(address);

            await _restaurantRepository.SaveAsync();

            var restaurant = new Restaurant
            {
                Name = request.RestaurantName,
                Email = request.RestaurantEmail,
                IsActive = true,
                AddressId = address.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _restaurantRepository.AddRestaurant(restaurant);

            await _restaurantRepository.SaveAsync();

            var owner = new User
            {
                Name = request.OwnerName,
                Email = request.OwnerEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
                Role = UserRole.admin.ToString(),
                IsActive = true,
                Balance = 1000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _userRepository.AddUser(owner);

            await _userRepository.SaveAsync();

            var restaurantOwner = new RestaurantOwner
            {
                UserId = owner.Id,
                RestaurantId = restaurant.Id,
                CreatedAt = DateTime.UtcNow
            };

            _restaurantRepository.AddRestaurantOwner(restaurantOwner);

            await _restaurantRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Restaurant and owner onboarded successfully.",
                Data = new
                {
                    RestaurantId = restaurant.Id,
                    OwnerId = owner.Id
                }
            };
        }

        public async Task<ApiResponse<object>> AddRestaurantOwnerAsync(AddRestaurantOwnerRequestDto request)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var response = await AddRestaurantOwnerInternalAsync(request);

                    if (!response.Success)
                    {
                        transaction.Rollback();
                        return response;
                    }

                    transaction.Commit();

                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<ApiResponse<object>> AddRestaurantOwnerInternalAsync(AddRestaurantOwnerRequestDto request)
        {
            var restaurant = await _restaurantRepository
                    .GetRestaurantByIdAsync(request.RestaurantId);

            if (restaurant == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Restaurant not found.",
                    Data = null
                };
            }

            if (!restaurant.IsActive)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Restaurant is inactive.",
                    Data = null
                };
            }

            if (restaurant.Email.Equals(
                request.OwnerEmail,
                StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Restaurant email and owner email must be different.",
                    Data = null
                };
            }

            var existingUser = await _userRepository
                    .GetUserByEmailAsync(request.OwnerEmail);

            if (existingUser != null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User with this email already exists.",
                    Data = null
                };
            }

            var owner = new User
            {
                Name = request.OwnerName,
                Email = request.OwnerEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
                Role = UserRole.admin.ToString(),
                IsActive = true,
                Balance = 1000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _userRepository.AddUser(owner);

            await _userRepository.SaveAsync();

            var restaurantOwner = new RestaurantOwner
            {
                UserId = owner.Id,
                RestaurantId = restaurant.Id,
                CreatedAt = DateTime.UtcNow
            };

            _restaurantRepository.AddRestaurantOwner(restaurantOwner);

            await _restaurantRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Restaurant owner onboarded successfully.",
                Data = new
                {
                    OwnerId = owner.Id,
                    RestaurantId = restaurant.Id
                }
            };
        }

    }
}
