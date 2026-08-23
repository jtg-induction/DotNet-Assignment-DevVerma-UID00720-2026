using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IReportingService
    {
        Task<ApiResponse<byte[]>> GetTopOrderedItemsAsync(long adminId, TopOrderedItemsRequestDto request);

        Task<ApiResponse<byte[]>> GetFrequentlyBoughtTogetherAsync(long adminId, int restaurantId);
    }
}
