using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using System;
using System.Collections.Generic;
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
                    var user = await _orderRepository
                            .GetUserForUpdateAsync(userId);

                    if (user == null)
                    {
                        transaction.Rollback();

                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = "User not found.",
                            Data = null
                        };
                    }

                    if (!user.IsActive)
                    {
                        transaction.Rollback();

                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = "User account is inactive.",
                            Data = null
                        };
                    }

                    var address = await _orderRepository
                            .GetAddressForUserAsync(request.AddressId,userId);

                    if (address == null)
                    {
                        transaction.Rollback();

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
                        transaction.Rollback();

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
                        transaction.Rollback();

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
                            transaction.Rollback();

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
                        transaction.Rollback();

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
                        Status = "placed",
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

                    transaction.Commit();

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
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
