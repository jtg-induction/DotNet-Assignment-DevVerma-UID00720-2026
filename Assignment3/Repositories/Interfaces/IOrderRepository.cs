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
    }
}
