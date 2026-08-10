using Assignment3.Data;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Implementations
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly FoodOrderingContext _context;

        public RestaurantRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<List<Restaurant>> GetAvailableRestaurantsAsync()
        {
            return await _context.Restaurants
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetAvailableMenuItemsAsync(int restaurantId)
        {
            return await _context.MenuItems
                .Where(m => m.RestaurantId == restaurantId)
                .Where(m => m.Restaurant.IsActive)
                .ToListAsync();
        }
    }
}
