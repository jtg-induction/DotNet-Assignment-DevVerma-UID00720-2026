using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Enums;
using Assignment3.Repositories.Implementations;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using System;
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

            if (string.Equals(
                request.Status,
                "cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Owner cannot cancel an order.",
                    Data = null
                };
            }

            var currentStatus =
                order.Status.ToLower();

            var requestedStatus =
                request.Status.ToLower();


            // REJECTED
            if (requestedStatus == "rejected")
            {
                if (currentStatus == "cancelled" ||
                    currentStatus == "delivered" ||
                    currentStatus == "rejected")
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Order status cannot be changed.",
                        Data = null
                    };
                }

                var response = await _orderService
                        .CancelOrderInternalAsync(
                            order.Id,
                            userId,
                            UserRole.admin.ToString());

                return response;
            }


            // ACCEPTED
            if (requestedStatus == "accepted")
            {
                if (currentStatus != "placed")
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Order can only be accepted when its current status is placed.",
                        Data = null
                    };
                }
            }

            // DISPATCHED
            else if (requestedStatus == "dispatched")
            {
                if (currentStatus != "accepted")
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Order can only be dispatched when its current status is accepted.",
                        Data = null
                    };
                }
            }

            // DELIVERED
            else if (requestedStatus == "delivered")
            {
                if (currentStatus != "dispatched")
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Order can only be delivered when its current status is dispatched.",
                        Data = null
                    };
                }
            }

            // INVALID STATUS
            else
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid order status.",
                    Data = null
                };
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
