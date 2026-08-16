using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Implementations;
using Assignment3.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Tests.Services
{
    [TestClass]
    public class OrderServiceTests
    {
        private Mock<IOrderRepository> _orderRepositoryMock;
        private Mock<FoodOrderingContext> _contextMock;

        private OrderService _orderService;

        [TestInitialize]
        public void Setup()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();

            _contextMock = new Mock<FoodOrderingContext>();

            _orderService = new OrderService(
                _contextMock.Object,
                _orderRepositoryMock.Object
            );
        }


        // CREATE ORDER - VALIDATION

        [TestMethod]
        public async Task CreateOrderAsync_EmptyItems_ReturnsFailure()
        {
            var request = new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1,
                Items = new List<CreateOrderItemDto>()
            };

            var result =
                await _orderService.CreateOrderAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order must contain at least one item.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetUserForUpdateAsync(It.IsAny<long>()),
                Times.Never);
        }


        [TestMethod]
        public async Task CreateOrderAsync_InvalidQuantity_ReturnsFailure()
        {
            var request = new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        MenuItemId = 1,
                        Quantity = 0
                    }
                }
            };

            var result =
                await _orderService.CreateOrderAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Quantity must be greater than zero.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetUserForUpdateAsync(It.IsAny<long>()),
                Times.Never);
        }


        [TestMethod]
        public async Task CreateOrderAsync_DuplicateItems_ReturnsFailure()
        {
            var request = new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        MenuItemId = 1,
                        Quantity = 2
                    },
                    new CreateOrderItemDto
                    {
                        MenuItemId = 1,
                        Quantity = 1
                    }
                }
            };

            var result =
                await _orderService.CreateOrderAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Duplicate menu items are not allowed.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetUserForUpdateAsync(It.IsAny<long>()),
                Times.Never);
        }


        // PLACE ORDER INTERNAL - BUSINESS LOGIC

        [TestMethod]
        public async Task PlaceOrderInternalAsync_UserDoesNotExist_ReturnsFailure()
        {
            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync((User)null);

            var request = CreateValidCreateOrderRequest();

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "User not found.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetUserForUpdateAsync(1),
                Times.Once);
        }


        [TestMethod]
        public async Task PlaceOrderInternalAsync_InactiveUser_ReturnsFailure()
        {
            var user = new User
            {
                Id = 1,
                IsActive = false,
                Balance = 1000
            };

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            var request = CreateValidCreateOrderRequest();

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "User account is inactive.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetAddressForUserAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>()),
                Times.Never);
        }


        [TestMethod]
        public async Task PlaceOrderInternalAsync_InvalidAddress_ReturnsFailure()
        {
            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 1000
            };

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetAddressForUserAsync(1, 1))
                .ReturnsAsync((Address)null);

            var request = CreateValidCreateOrderRequest();

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Invalid address.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()),
                Times.Never);
        }


        [TestMethod]
        public async Task PlaceOrderInternalAsync_MenuItemDoesNotExist_ReturnsFailure()
        {
            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 1000
            };

            var address = CreateAddress();

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetAddressForUserAsync(1, 1))
                .ReturnsAsync(address);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()))
                .ReturnsAsync(new List<MenuItem>());

            var request = CreateValidCreateOrderRequest();

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "One or more menu items were not found.",
                result.Message);
        }


        [TestMethod]
        public async Task PlaceOrderInternalAsync_MenuItemBelongsToDifferentRestaurant_ReturnsFailure()
        {
            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 1000
            };

            var address = CreateAddress();

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 2,
                    Name = "Pizza",
                    Price = 200,
                    Stock = 10
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetAddressForUserAsync(1, 1))
                .ReturnsAsync(address);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()))
                .ReturnsAsync(menuItems);

            var request = CreateValidCreateOrderRequest();

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "All menu items must belong to the selected restaurant.",
                result.Message);
        }


        [TestMethod]
        public async Task PlaceOrderInternalAsync_InsufficientStock_ReturnsFailure()
        {
            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 1000
            };

            var address = CreateAddress();

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 1,
                    Name = "Pizza",
                    Price = 200,
                    Stock = 1
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetAddressForUserAsync(1, 1))
                .ReturnsAsync(address);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()))
                .ReturnsAsync(menuItems);

            var request = CreateValidCreateOrderRequest();
            request.Items[0].Quantity = 5;

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Insufficient stock for Pizza.",
                result.Message);
        }


        [TestMethod]
        public async Task PlaceOrderInternalAsync_InsufficientBalance_ReturnsFailure()
        {
            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 100
            };

            var address = CreateAddress();

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 1,
                    Name = "Pizza",
                    Price = 200,
                    Stock = 10
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetAddressForUserAsync(1, 1))
                .ReturnsAsync(address);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()))
                .ReturnsAsync(menuItems);

            var request = CreateValidCreateOrderRequest();

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Insufficient wallet balance.",
                result.Message);
        }


        [TestMethod]
        public async Task PlaceOrderInternalAsync_Success_CreatesOrderAndOrderItems()
        {
            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 1000
            };

            var address = CreateAddress();

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 1,
                    Name = "Pizza",
                    Price = 200,
                    Stock = 10
                },
                new MenuItem
                {
                    Id = 2,
                    RestaurantId = 1,
                    Name = "Burger",
                    Price = 150,
                    Stock = 5
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetAddressForUserAsync(1, 1))
                .ReturnsAsync(address);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()))
                .ReturnsAsync(menuItems);

            _orderRepositoryMock
                .Setup(x => x.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) =>
                {
                    order.Id = 100;
                    return order;
                });

            _orderRepositoryMock
                .Setup(x => x.CreateOrderItemsAsync(
                    It.IsAny<List<OrderItem>>()))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var request = new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        MenuItemId = 1,
                        Quantity = 2
                    },
                    new CreateOrderItemDto
                    {
                        MenuItemId = 2,
                        Quantity = 1
                    }
                }
            };

            var result =
                await _orderService.PlaceOrderInternalAsync(1, request);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order placed successfully.",
                result.Message);

            Assert.AreEqual(
                450,
                user.Balance);

            Assert.AreEqual(
                8,
                menuItems[0].Stock);

            Assert.AreEqual(
                4,
                menuItems[1].Stock);

            _orderRepositoryMock.Verify(
                x => x.CreateOrderAsync(
                    It.Is<Order>(o =>
                        o.UserId == 1 &&
                        o.RestaurantId == 1 &&
                        o.Status == OrderStatus.placed.ToString() &&
                        o.TotalAmount == 550 &&
                        o.BuildingNumber == address.BuildingNumber &&
                        o.City == address.City)),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.CreateOrderItemsAsync(
                    It.Is<List<OrderItem>>(items =>
                        items.Count == 2 &&
                        items.Any(i =>
                            i.MenuItemId == 1 &&
                            i.Quantity == 2 &&
                            i.UnitPrice == 200) &&
                        items.Any(i =>
                            i.MenuItemId == 2 &&
                            i.Quantity == 1 &&
                            i.UnitPrice == 150))),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }


        // GET ORDER DETAILS

        [TestMethod]
        public async Task GetOrderDetailsAsync_OrderDoesNotExist_ReturnsFailure()
        {
            _orderRepositoryMock
                .Setup(x => x.GetOrderDetailsAsync(100, 1))
                .ReturnsAsync((Order)null);

            var result =
                await _orderService.GetOrderDetailsAsync(100, 1);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order not found.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetOrderDetailsAsync(100, 1),
                Times.Once);
        }


        [TestMethod]
        public async Task GetOrderDetailsAsync_ValidOrder_ReturnsDetails()
        {
            var order = new Order
            {
                Id = 100,
                UserId = 1,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString(),
                TotalAmount = 400,

                BuildingNumber = "10",
                Locality = "Sector 1",
                City = "Delhi",
                State = "Delhi",
                Country = "India",
                PostalCode = "110001",

                CreatedAt = DateTime.UtcNow,

                Restaurant = new Restaurant
                {
                    Id = 1,
                    Name = "Pizza Palace"
                },

                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        MenuItemId = 1,
                        Quantity = 2,
                        UnitPrice = 200,

                        MenuItem = new MenuItem
                        {
                            Id = 1,
                            Name = "Pizza"
                        }
                    }
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderDetailsAsync(100, 1))
                .ReturnsAsync(order);

            var result =
                await _orderService.GetOrderDetailsAsync(100, 1);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order details retrieved successfully.",
                result.Message);

            var data =
                (OrderDetailsDto)result.Data;

            Assert.AreEqual(100, data.OrderId);
            Assert.AreEqual(
                OrderStatus.placed.ToString(),
                data.Status);
            Assert.AreEqual(
                "Pizza Palace",
                data.RestaurantName);
            Assert.AreEqual(400, data.TotalAmount);

            Assert.AreEqual(
                "10",
                data.BuildingNumber);
            Assert.AreEqual(
                "Sector 1",
                data.Locality);
            Assert.AreEqual(
                "Delhi",
                data.City);
            Assert.AreEqual(
                "Delhi",
                data.State);
            Assert.AreEqual(
                "India",
                data.Country);
            Assert.AreEqual(
                "110001",
                data.PostalCode);

            Assert.IsNotNull(data.Items);

            Assert.AreEqual(1, data.Items.Count);

            Assert.AreEqual(
                1,
                data.Items[0].MenuItemId);

            Assert.AreEqual(
                "Pizza",
                data.Items[0].Name);

            Assert.AreEqual(
                2,
                data.Items[0].Quantity);

            Assert.AreEqual(
                200,
                data.Items[0].UnitPrice);

            Assert.AreEqual(
                400,
                data.Items[0].Subtotal);

            _orderRepositoryMock.Verify(
                x => x.GetOrderDetailsAsync(100, 1),
                Times.Once);
        }


        // CANCEL ORDER

        [TestMethod]
        public async Task CancelOrderInternalAsync_OrderDoesNotExist_ReturnsFailure()
        {
            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync((Order)null);

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order not found.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetOrderForUpdateAsync(100),
                Times.Once);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_DifferentUser_ReturnsUnauthorized()
        {
            var order = CreatePlacedOrder();

            order.UserId = 2;

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "You are not authorized to cancel this order.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetOrderItemsAsync(It.IsAny<long>()),
                Times.Never);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_AdminNotRestaurantOwner_ReturnsUnauthorized()
        {
            var order = CreatePlacedOrder();

            order.UserId = 2;

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(1, 1))
                .ReturnsAsync(false);

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.admin.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "You are not authorized to cancel this order.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(1, 1),
                Times.Once);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_AdminRestaurantOwner_IsAuthorized()
        {
            var order = CreatePlacedOrder();

            order.UserId = 2;

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(1, 1))
                .ReturnsAsync(true);

            _orderRepositoryMock
                .Setup(x => x.GetOrderItemsAsync(100))
                .ReturnsAsync(new List<OrderItem>());

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.admin.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order items not found.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(1, 1),
                Times.Once);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_NonAdminDoesNotCheckRestaurantOwner()
        {
            var order = CreatePlacedOrder();

            order.UserId = 2;

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "You are not authorized to cancel this order.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(
                    It.IsAny<long>(),
                    It.IsAny<int>()),
                Times.Never);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_AcceptedOrder_ReturnsFailure()
        {
            await AssertInvalidOrderStatus(OrderStatus.accepted.ToString());
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_RejectedOrder_ReturnsFailure()
        {
            await AssertInvalidOrderStatus(OrderStatus.rejected.ToString());
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_DispatchedOrder_ReturnsFailure()
        {
            await AssertInvalidOrderStatus(OrderStatus.dispatched.ToString());
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_DeliveredOrder_ReturnsFailure()
        {
            await AssertInvalidOrderStatus(OrderStatus.delivered.ToString());
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_CancelledOrder_ReturnsFailure()
        {
            await AssertInvalidOrderStatus(OrderStatus.cancelled.ToString());
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_OrderItemsDoNotExist_ReturnsFailure()
        {
            var order = CreatePlacedOrder();

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.GetOrderItemsAsync(100))
                .ReturnsAsync(new List<OrderItem>());

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order items not found.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetUserForUpdateAsync(
                    It.IsAny<long>()),
                Times.Never);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_OrderUserDoesNotExist_ReturnsFailure()
        {
            var order = CreatePlacedOrder();

            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    MenuItemId = 1,
                    Quantity = 2,
                    UnitPrice = 200
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.GetOrderItemsAsync(100))
                .ReturnsAsync(orderItems);

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync((User)null);

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order user not found.",
                result.Message);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_MenuItemDoesNotExist_ReturnsFailure()
        {
            var order = CreatePlacedOrder();

            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    MenuItemId = 1,
                    Quantity = 2,
                    UnitPrice = 200
                }
            };

            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 500
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.GetOrderItemsAsync(100))
                .ReturnsAsync(orderItems);

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()))
                .ReturnsAsync(new List<MenuItem>());

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "One or more menu items were not found.",
                result.Message);
        }


        [TestMethod]
        public async Task CancelOrderInternalAsync_Success_RefundsBalanceRestoresStockAndCancelsOrder()
        {
            var order = CreatePlacedOrder();

            order.TotalAmount = 400;

            var user = new User
            {
                Id = 1,
                IsActive = true,
                Balance = 100
            };

            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    MenuItemId = 1,
                    Quantity = 2,
                    UnitPrice = 200
                },
                new OrderItem
                {
                    MenuItemId = 2,
                    Quantity = 1,
                    UnitPrice = 100
                }
            };

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 1,
                    Name = "Pizza",
                    Price = 200,
                    Stock = 3
                },
                new MenuItem
                {
                    Id = 2,
                    RestaurantId = 1,
                    Name = "Burger",
                    Price = 100,
                    Stock = 4
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.GetOrderItemsAsync(100))
                .ReturnsAsync(orderItems);

            _orderRepositoryMock
                .Setup(x => x.GetUserForUpdateAsync(1))
                .ReturnsAsync(user);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemsForUpdateAsync(
                    It.IsAny<List<long>>()))
                .ReturnsAsync(menuItems);

            _orderRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order cancelled successfully.",
                result.Message);

            Assert.AreEqual(
                OrderStatus.cancelled.ToString(),
                order.Status);

            Assert.AreEqual(
                500,
                user.Balance);

            Assert.AreEqual(
                5,
                menuItems[0].Stock);

            Assert.AreEqual(
                5,
                menuItems[1].Stock);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }


        // HELPER METHODS

        private CreateOrderRequestDto CreateValidCreateOrderRequest()
        {
            return new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        MenuItemId = 1,
                        Quantity = 1
                    }
                }
            };
        }


        private Address CreateAddress()
        {
            return new Address
            {
                Id = 1,
                UserId = 1,
                BuildingNumber = "10",
                Locality = "Sector 1",
                City = "Delhi",
                State = "Delhi",
                Country = "India",
                PostalCode = "110001"
            };
        }


        private Order CreatePlacedOrder()
        {
            return new Order
            {
                Id = 100,
                UserId = 1,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString(),
                TotalAmount = 400,
                BuildingNumber = "10",
                Locality = "Sector 1",
                City = "Delhi",
                State = "Delhi",
                Country = "India",
                PostalCode = "110001",
                CreatedAt = DateTime.UtcNow
            };
        }


        private async Task AssertInvalidOrderStatus(string status)
        {
            var order = CreatePlacedOrder();
            order.Status = status;

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            var result =
                await _orderService.CancelOrderInternalAsync(
                    100,
                    1,
                    UserRole.customer.ToString());

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Only orders in placed status can be cancelled.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.GetOrderItemsAsync(
                    It.IsAny<long>()),
                Times.Never);
        }
    }
}
