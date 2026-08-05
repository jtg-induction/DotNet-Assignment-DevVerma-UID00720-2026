using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using System.Threading.Tasks;

namespace Assignment3.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<object>> SignUpAsync(SignUpRequestDto request);
    }
}
