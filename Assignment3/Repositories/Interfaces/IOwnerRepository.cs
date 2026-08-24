using Assignment3.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Interfaces
{
    public interface IOwnerRepository
    {
        Task<(List<OwnerOrderDto> Orders, int TotalCount)> GetOwnerOrdersAsync(
            long ownerId,
            OwnerOrderDashboardRequestDto request);
    }
}
