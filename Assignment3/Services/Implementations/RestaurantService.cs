using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<ApiResponse<object>> GetAvailableRestaurantsAsync(int page, int pageSize)
        {
            var result = await _restaurantRepository.GetAvailableRestaurantsAsync(page, pageSize);

            var restaurantDtos = result.Restaurants.Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                Email = r.Email,
                AddressId = r.AddressId
            }).ToList();

            var totalPages = (int)System.Math.Ceiling(
                (double)result.TotalCount / pageSize);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Restaurants retrieved successfully.",
                Data = new
                {
                    Restaurants = restaurantDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = result.TotalCount,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<ApiResponse<object>> GetAvailableMenuItemsAsync(int restaurantId)
        {
            var menuItems = await _restaurantRepository.GetAvailableMenuItemsAsync(restaurantId);

            var menuItemDtos = menuItems.Select(m => new MenuItemDto
            {
                Id = m.Id,
                RestaurantId = m.RestaurantId,
                Name = m.Name,
                Price = m.Price,
                Description = m.Description,
                Stock = m.Stock
            }).ToList();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Menu items retrieved successfully.",
                Data = menuItemDtos
            };
        }
    }
}
