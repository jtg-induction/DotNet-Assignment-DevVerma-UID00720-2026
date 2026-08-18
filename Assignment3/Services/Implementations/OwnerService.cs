using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Enums;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Services.Implementations
{
    public class OwnerService : IOwnerService
    {
        private readonly IOwnerRepository _ownerRepository;

        public OwnerService(IOwnerRepository ownerRepository)
        {
            _ownerRepository = ownerRepository;
        }

        public async Task<ApiResponse<object>> GetOwnerOrdersAsync(
            long ownerId,
            OwnerOrderDashboardRequestDto request)
        {
            if (request.Page < 1)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Page must be greater than 0.",
                    Data = null
                };
            }

            if (request.PageSize < 1 || request.PageSize > 50)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "PageSize must be between 1 and 50.",
                    Data = null
                };
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                var validStatuses = new[]
                {
                    OrderStatus.placed.ToString(),
                    OrderStatus.accepted.ToString(),
                    OrderStatus.rejected.ToString(),
                    OrderStatus.dispatched.ToString(),
                    OrderStatus.delivered.ToString(),
                    OrderStatus.cancelled.ToString()
                };

                if (!validStatuses.Contains(request.Status))
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid order status.",
                        Data = null
                    };
                }
            }

            if (request.SortBy != "amount" &&
                request.SortBy != "createdAt")
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "SortBy must be either amount or createdAt.",
                    Data = null
                };
            }

            if (request.SortOrder != "asc" &&
                request.SortOrder != "desc")
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "SortOrder must be either asc or desc.",
                    Data = null
                };
            }

            if (request.FromDate.HasValue &&
                request.ToDate.HasValue &&
                request.FromDate.Value.Date > request.ToDate.Value.Date)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "FromDate cannot be greater than ToDate.",
                    Data = null
                };
            }

            var result = await _ownerRepository
                .GetOwnerOrdersAsync(ownerId, request);

            var totalPages = (int)Math.Ceiling(
                (double)result.TotalCount / request.PageSize);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Orders retrieved successfully.",
                Data = new OwnerOrderDashboardDto
                {
                    Orders = result.Orders,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = result.TotalCount,
                    TotalPages = totalPages
                }
            };
        }
    }
}
