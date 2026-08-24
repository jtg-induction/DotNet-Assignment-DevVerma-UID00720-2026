using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Enums;
using Assignment3.Repositories.Implementations;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Services.Implementations
{
    public class OwnerService : IOwnerService
    {
        private readonly FoodOrderingContext _context;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;

        public OwnerService(
            FoodOrderingContext context,
            IOwnerRepository ownerRepository, 
            IOrderService orderService,
            IOrderRepository orderRepository
            )
        {
            _context = context;
            _ownerRepository = ownerRepository;
            _orderService = orderService;
            _orderRepository = orderRepository;
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

        public async Task<ApiResponse<object>> UpdateOrderStatusAsync(
            UpdateOrderStatusRequestDto request,
            long userId)
        {
            using (var transaction =
                _context.Database.BeginTransaction())
            {
                try
                {
                    var response =
                        await UpdateOrderStatusInternalAsync(
                            request,
                            userId);

                    if (!response.Success)
                    {
                        transaction.Rollback();
                        return response;
                    }

                    transaction.Commit();

                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static readonly Dictionary<string, List<string>> AllowedTransitions = new Dictionary<string, List<string>>
        {
            {
                OrderStatus.placed.ToString(),
                new List<string>
                {
                    OrderStatus.accepted.ToString(),
                    OrderStatus.rejected.ToString()
                }
            },

            {
                OrderStatus.accepted.ToString(),
                new List<string>
                {
                    OrderStatus.dispatched.ToString(),
                    OrderStatus.rejected.ToString()
                }
            },

            {
                OrderStatus.dispatched.ToString(),
                new List<string>
                {
                    OrderStatus.delivered.ToString(),
                    OrderStatus.rejected.ToString()
                }
            },

            {
                OrderStatus.rejected.ToString(),
                new List<string>()
            },

            {
                OrderStatus.cancelled.ToString(),
                new List<string>()
            },

            {
                OrderStatus.delivered.ToString(),
                new List<string>()
            }
        };

        public async Task<ApiResponse<object>> UpdateOrderStatusInternalAsync(
            UpdateOrderStatusRequestDto request,
            long userId)
        {
            var order = await _orderRepository
                .GetOrderForUpdateAsync(request.OrderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            var isOwner = await _orderRepository
                .IsRestaurantOwnerAsync(
                    userId,
                    order.RestaurantId);

            if (!isOwner)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "You are not an owner of this restaurant.",
                    Data = null
                };
            }

            var currentStatus = order.Status.ToLower();

            var requestedStatus = request.Status.ToLower();

            if (!AllowedTransitions.ContainsKey(currentStatus) ||
                !AllowedTransitions[currentStatus].Contains(requestedStatus))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order status cannot be changed.",
                    Data = null
                };
            }

            if (requestedStatus == OrderStatus.rejected.ToString())
            {
                var response = await _orderService
                        .CancelOrderInternalAsync(
                            order.Id,
                            userId,
                            UserRole.admin.ToString());

                return response;
            }

            order.Status = requestedStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Order status updated successfully.",
                Data = new
                {
                    OrderId = order.Id,
                    Status = order.Status
                }
            };
        }

    }
}
