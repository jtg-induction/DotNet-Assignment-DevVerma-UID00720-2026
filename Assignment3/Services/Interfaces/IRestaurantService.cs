using Assignment3.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<ApiResponse<object>> GetAvailableRestaurantsAsync();
        Task<ApiResponse<object>> GetAvailableMenuItemsAsync(int restaurantId);
    }
}
