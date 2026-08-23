using Assignment3.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Interfaces
{
    public interface IReportingRepository
    {
        Task<List<TopOrderedItemDto>> GetTopOrderedItemsAsync(long adminId, TopOrderedItemsRequestDto request);

        Task<List<FrequentlyBoughtTogetherDto>> GetFrequentlyBoughtTogetherAsync(int restaurantId);
    }
}
