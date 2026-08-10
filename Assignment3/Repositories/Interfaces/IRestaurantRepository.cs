using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment3.Models;

namespace Assignment3.Repositories.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<List<Restaurant>> GetAvailableRestaurantsAsync();
        Task<List<MenuItem>> GetAvailableMenuItemsAsync(int restaurantId);
    }
}
