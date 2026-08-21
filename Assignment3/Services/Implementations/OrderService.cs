using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Enums;
using Assignment3.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly FoodOrderingContext _context;
        private readonly IOrderRepository _orderRepository;

        public OrderService(
            FoodOrderingContext context,
            IOrderRepository orderRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
        }

        public async Task<ApiResponse<object>> CreateOrderAsync(
            long userId,
            CreateOrderRequestDto request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order must contain at least one item.",
                    Data = null
                };
            }

            if (request.Items.Any(x => x.Quantity <= 0))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Quantity must be greater than zero.",
                    Data = null
                };
            }

            var duplicateItemIds = request.Items
                .GroupBy(x => x.MenuItemId)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateItemIds.Any())
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Duplicate menu items are not allowed.",
                    Data = null
                };
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var response = await PlaceOrderInternalAsync(
                        userId,
                        request);

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

        public async Task<ApiResponse<object>> PlaceOrderInternalAsync(
            long userId,
            CreateOrderRequestDto request)
        {
            var user = await _orderRepository
                .GetUserForUpdateAsync(userId);

            if (user == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                };
            }

            if (!user.IsActive)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User account is inactive.",
                    Data = null
                };
            }

            var address = await _orderRepository
                .GetAddressForUserAsync(request.AddressId, userId);

            if (address == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid address.",
                    Data = null
                };
            }

            var menuItemIds = request.Items
                .Select(x => x.MenuItemId)
                .ToList();

            var menuItems = await _orderRepository
                .GetMenuItemsForUpdateAsync(menuItemIds);

            if (menuItems.Count != menuItemIds.Count)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "One or more menu items were not found.",
                    Data = null
                };
            }

            if (menuItems.Any(x =>
                x.RestaurantId != request.RestaurantId))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "All menu items must belong to the selected restaurant.",
                    Data = null
                };
            }

            decimal totalAmount = 0;

            foreach (var requestItem in request.Items)
            {
                var menuItem = menuItems
                    .First(x =>
                        x.Id == requestItem.MenuItemId);

                if (menuItem.Stock < requestItem.Quantity)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message =
                            $"Insufficient stock for {menuItem.Name}.",
                        Data = null
                    };
                }

                totalAmount += menuItem.Price * requestItem.Quantity;
            }

            if (user.Balance < totalAmount)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Insufficient wallet balance.",
                    Data = null
                };
            }

            user.Balance -= totalAmount;
            user.UpdatedAt = DateTime.UtcNow;

            foreach (var requestItem in request.Items)
            {
                var menuItem = menuItems
                    .First(x =>
                        x.Id == requestItem.MenuItemId);

                menuItem.Stock -= requestItem.Quantity;
                menuItem.UpdatedAt = DateTime.UtcNow;
            }

            var order = new Order
            {
                UserId = userId,
                RestaurantId = request.RestaurantId,
                Status = OrderStatus.placed.ToString(),
                TotalAmount = totalAmount,
                BuildingNumber = address.BuildingNumber,
                Locality = address.Locality,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PostalCode = address.PostalCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _orderRepository.CreateOrderAsync(order);

            var orderItems = request.Items
                .Select(requestItem =>
                {
                    var menuItem = menuItems
                        .First(x =>
                            x.Id == requestItem.MenuItemId);

                    return new OrderItem
                    {
                        OrderId = order.Id,
                        MenuItemId = menuItem.Id,
                        Quantity = requestItem.Quantity,
                        UnitPrice = menuItem.Price,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = null
                    };
                })
                .ToList();

            await _orderRepository
                .CreateOrderItemsAsync(orderItems);

            await _orderRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Order placed successfully.",
                Data = new
                {
                    OrderId = order.Id
                }
            };
        }

        public async Task<ApiResponse<object>> GetOrderDetailsAsync(long orderId, long userId)
        {
            var order = await _orderRepository
                .GetOrderDetailsAsync(orderId, userId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            var orderDetails = new OrderDetailsDto
            {
                OrderId = order.Id,
                Status = order.Status,
                RestaurantName = order.Restaurant.Name,
                TotalAmount = order.TotalAmount,

                BuildingNumber = order.BuildingNumber,
                Locality = order.Locality,
                City = order.City,
                State = order.State,
                Country = order.Country,
                PostalCode = order.PostalCode,

                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                Items = order.OrderItems.Select(oi => new OrderItemDetailsDto
                {
                    MenuItemId = oi.MenuItemId,
                    Name = oi.MenuItem.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Order details retrieved successfully.",
                Data = orderDetails
            };
        }

        public async Task<ApiResponse<object>> CancelOrderAsync(long orderId, long userId, string role)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var response = await CancelOrderInternalAsync(
                        orderId,
                        userId,
                        role);

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

        public async Task<ApiResponse<object>> CancelOrderInternalAsync(
            long orderId,
            long userId,
            string role)
        {
            var order = await _orderRepository
                .GetOrderForUpdateAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            bool isCustomer = order.UserId == userId;

            bool isRestaurantOwner = false;

            if (!isCustomer && role == UserRole.admin.ToString())
            {
                isRestaurantOwner =
                    await _orderRepository.IsRestaurantOwnerAsync(
                        userId,
                        order.RestaurantId);
            }

            if (!isCustomer && !isRestaurantOwner)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "You are not authorized to cancel this order.",
                    Data = null
                };
            }

            if (order.Status != OrderStatus.placed.ToString() && isCustomer)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Only orders in placed status can be cancelled.",
                    Data = null
                };
            }

            if ((order.Status == OrderStatus.delivered.ToString() ||
                order.Status == OrderStatus.cancelled.ToString() ||
                order.Status == OrderStatus.rejected.ToString()) 
                && isRestaurantOwner
                )
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order cannot be rejected",
                    Data = null
                };
            }

            var orderItems = await _orderRepository
                .GetOrderItemsAsync(orderId);

            if (!orderItems.Any())
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order items not found.",
                    Data = null
                };
            }

            var user = await _orderRepository
                .GetUserForUpdateAsync(order.UserId);

            if (user == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order user not found.",
                    Data = null
                };
            }

            var menuItemIds = orderItems
                .Select(oi => oi.MenuItemId)
                .Distinct()
                .ToList();

            var menuItems = await _orderRepository
                .GetMenuItemsForUpdateAsync(menuItemIds);

            if (menuItems.Count != menuItemIds.Count)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "One or more menu items were not found.",
                    Data = null
                };
            }

            user.Balance += order.TotalAmount;
            user.UpdatedAt = DateTime.UtcNow;

            foreach (var orderItem in orderItems)
            {
                var menuItem = menuItems
                    .First(m => m.Id == orderItem.MenuItemId);

                menuItem.Stock += orderItem.Quantity;
                menuItem.UpdatedAt = DateTime.UtcNow;
            }

            order.Status = isRestaurantOwner ?
                OrderStatus.rejected.ToString() : OrderStatus.cancelled.ToString();
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Order cancelled successfully.",
                Data = new
                {
                    OrderId = order.Id,
                    RefundedAmount = order.TotalAmount
                }
            };
        }
    }
}
