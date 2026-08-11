using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment3.Models;

namespace Assignment3.Repositories.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<(List<Restaurant> Restaurants, int TotalCount)> GetAvailableRestaurantsAsync(int page , int pageSize);
        Task<List<MenuItem>> GetAvailableMenuItemsAsync(int restaurantId);
    }
}
