using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface ISuperAdminService
    {
        Task<ApiResponse<object>> AddRestaurantAsync(
            AddRestaurantRequestDto request);

        Task<ApiResponse<object>> AddRestaurantOwnerAsync(
            AddRestaurantOwnerRequestDto request);
    }
}
