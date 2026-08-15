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


        // CREATE ORDER TESTS

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


        // GET ORDER DETAILS TESTS

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

            // Order information
            Assert.AreEqual(100,data.OrderId);

            Assert.AreEqual(OrderStatus.placed.ToString(),data.Status);

            Assert.AreEqual("Pizza Palace",data.RestaurantName);

            Assert.AreEqual(400,data.TotalAmount);

            // Address information
            Assert.AreEqual("10",data.BuildingNumber);

            Assert.AreEqual("Sector 1",data.Locality);

            Assert.AreEqual( "Delhi",data.City);

            Assert.AreEqual("Delhi",data.State);

            Assert.AreEqual("India", data.Country);

            Assert.AreEqual("110001",data.PostalCode);

            // Order items
            Assert.IsNotNull(data.Items);

            Assert.AreEqual(1, data.Items.Count);

            Assert.AreEqual(1, data.Items[0].MenuItemId);

            Assert.AreEqual("Pizza", data.Items[0].Name);

            Assert.AreEqual(2, data.Items[0].Quantity);

            Assert.AreEqual(200, data.Items[0].UnitPrice);

            Assert.AreEqual(400, data.Items[0].Subtotal);

            _orderRepositoryMock.Verify(
                x => x.GetOrderDetailsAsync(100, 1),
                Times.Once);
        }
    }
}
