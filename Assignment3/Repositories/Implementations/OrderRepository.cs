using Assignment3.Data;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FoodOrderingContext _context;

        public OrderRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserForUpdateAsync(long userId)
        {
            var user = await _context.Database
                .SqlQuery<User>(
                    @"SELECT *
                      FROM Users WITH (UPDLOCK, ROWLOCK)
                      WHERE Id = @p0",
                    userId)
                .FirstOrDefaultAsync();

            if(user != null)
            {
                _context.Users.Attach(user);
            }

            return user;
        }

        public async Task<Address> GetAddressForUserAsync(
            long addressId,
            long userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a =>
                    a.Id == addressId &&
                    a.UserId == userId);
        }

        public async Task<List<MenuItem>> GetMenuItemsForUpdateAsync(
            List<long> menuItemIds)
        {
            if (menuItemIds == null || !menuItemIds.Any())
                return new List<MenuItem>();

            var ids = string.Join(",", menuItemIds);

            var menuItems = await _context.Database
                .SqlQuery<MenuItem>(
                    $@"SELECT *
                       FROM MenuItems WITH (UPDLOCK, ROWLOCK)
                       WHERE Id IN ({ids})")
                .ToListAsync();

            foreach(var menuItem in menuItems)
            {
                _context.MenuItems.Attach(menuItem);
            }

            return menuItems;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            return order;
        }

        public async Task CreateOrderItemsAsync(
            List<OrderItem> orderItems)
        {
            _context.OrderItems.AddRange(orderItems);

            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderDetailsAsync(long orderId, long userId)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems.Select(oi => oi.MenuItem))
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == userId);
        }

        public async Task<Order> GetOrderForUpdateAsync(long orderId)
        {
            var order = await _context.Database
                .SqlQuery<Order>(
                    @"SELECT * FROM Orders WITH (UPDLOCK, ROWLOCK) WHERE Id = @p0",
                    orderId)
                .FirstOrDefaultAsync();

            if (order != null)
            {
                _context.Orders.Attach(order);
            }

            return order;
        }

        public async Task<bool> IsRestaurantOwnerAsync(long userId, int restaurantId)
        {
            return await _context.RestaurantOwners
                .AnyAsync(ro =>
                    ro.UserId == userId &&
                    ro.RestaurantId == restaurantId);
        }
    }
}
