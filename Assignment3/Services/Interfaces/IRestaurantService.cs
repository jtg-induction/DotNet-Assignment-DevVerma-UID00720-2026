using Assignment3.DTOs.Common;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<ApiResponse<object>> GetAvailableRestaurantsAsync(int page, int pageSize);
        Task<ApiResponse<object>> GetAvailableMenuItemsAsync(int restaurantId);
    }
}
