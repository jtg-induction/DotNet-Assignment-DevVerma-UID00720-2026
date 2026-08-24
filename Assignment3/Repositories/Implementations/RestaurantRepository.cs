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

        public async Task<(List<Restaurant> Restaurants, int TotalCount)> GetAvailableRestaurantsAsync(int page,int pageSize)
        {
            var query = _context.Restaurants
                .AsNoTracking()
                .Where(r => r.IsActive);

            var totalCount = await query.CountAsync();

            var restaurants = await query
                .OrderBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (restaurants, totalCount);
        }

        public async Task<List<MenuItem>> GetAvailableMenuItemsAsync(int restaurantId)
        {
            return await _context.MenuItems
                .AsNoTracking()
                .Where(m => m.RestaurantId == restaurantId)
                .Where(m => m.Restaurant.IsActive)
                .ToListAsync();
        }

        public async Task<Restaurant> GetRestaurantByEmailAsync(string email)
        {
            return await _context.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Email == email);
        }

        public async Task<Restaurant> GetRestaurantByIdAsync(int restaurantId)
        {
            return await _context.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
        }

        public async Task<bool> IsRestaurantOwnerAsync(long userId,int restaurantId)
        {
            return await _context.RestaurantOwners
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.RestaurantId == restaurantId);
        }

        public void AddRestaurant(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
        }

        public void AddAddress(Address address)
        {
            _context.Addresses.Add(address);
        }

        public void AddRestaurantOwner(RestaurantOwner restaurantOwner)
        {
            _context.RestaurantOwners.Add(restaurantOwner);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
