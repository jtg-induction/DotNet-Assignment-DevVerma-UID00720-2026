using Assignment3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<User> GetUserForUpdateAsync(long userId);

        Task<Address> GetAddressForUserAsync(long addressId, long userId);

        Task<List<MenuItem>> GetMenuItemsForUpdateAsync(
            List<long> menuItemIds);

        Task<Order> CreateOrderAsync(Order order);

        Task CreateOrderItemsAsync(List<OrderItem> orderItems);

        Task SaveAsync();

        Task<Order> GetOrderDetailsAsync(long orderId, long userId);

        Task<Order> GetOrderForUpdateAsync(long orderId);

        Task<bool> IsRestaurantOwnerAsync(long userId, int restaurantId);
    }
}
