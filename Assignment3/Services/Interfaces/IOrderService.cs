using Assignment3.DTOs.Common;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<object>> CreateOrderAsync(long userId, CreateOrderRequestDto request);

        Task<ApiResponse<object>> GetOrderDetailsAsync(long orderId, long userId);

        Task<ApiResponse<object>> CancelOrderAsync(long orderId, long userId, string role);
    }
}
