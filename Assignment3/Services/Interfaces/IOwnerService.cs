using Assignment3.DTOs.Common;
using Assignment3.DTOs;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IOwnerService
    {
        Task<ApiResponse<object>> GetOwnerOrdersAsync(
            long ownerId,
            OwnerOrderDashboardRequestDto request);
    }
}
