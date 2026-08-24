using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Enums;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Implementations;
using Assignment3.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment3.Tests.Services
{
    [TestClass]
    public class OwnerServiceTests
    {
        private Mock<FoodOrderingContext> _contextMock;
        private Mock<IOwnerRepository> _ownerRepositoryMock;
        private Mock<IOrderService> _orderServiceMock;
        private Mock<IOrderRepository> _orderRepositoryMock;

        private OwnerService _ownerService;

        [TestInitialize]
        public void Setup()
        {
            _contextMock =
                new Mock<FoodOrderingContext>();

            _ownerRepositoryMock =
                new Mock<IOwnerRepository>();

            _orderServiceMock =
                new Mock<IOrderService>();

            _orderRepositoryMock =
                new Mock<IOrderRepository>();

            _ownerService =
                new OwnerService(
                    _contextMock.Object,
                    _ownerRepositoryMock.Object,
                    _orderServiceMock.Object,
                    _orderRepositoryMock.Object);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_InvalidPage_ReturnsFailure()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 0,
                PageSize = 10
            };

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Page must be greater than 0.",
                result.Message);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_PageSizeZero_ReturnsFailure()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 0
            };

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "PageSize must be between 1 and 50.",
                result.Message);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_PageSizeGreaterThan50_ReturnsFailure()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 51
            };

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "PageSize must be between 1 and 50.",
                result.Message);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_InvalidStatus_ReturnsFailure()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10,
                Status = "invalid"
            };

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Invalid order status.",
                result.Message);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_ValidStatus_CallsRepository()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10,
                Status = OrderStatus.placed.ToString()
            };

            _ownerRepositoryMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    1,
                    It.IsAny<OwnerOrderDashboardRequestDto>()))
                .ReturnsAsync((
                    new List<OwnerOrderDto>(),
                    0));

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsTrue(result.Success);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    1,
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Once);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_InvalidSortBy_ReturnsFailure()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10,
                SortBy = "name"
            };

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "SortBy must be either amount or createdAt.",
                result.Message);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_InvalidSortOrder_ReturnsFailure()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10,
                SortBy = "amount",
                SortOrder = "random"
            };

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "SortOrder must be either asc or desc.",
                result.Message);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_FromDateGreaterThanToDate_ReturnsFailure()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10,
                FromDate = new DateTime(2026, 8, 20),
                ToDate = new DateTime(2026, 8, 10)
            };

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "FromDate cannot be greater than ToDate.",
                result.Message);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_Success_ReturnsOrders()
        {
            var orders = new List<OwnerOrderDto>
            {
                new OwnerOrderDto
                {
                    OrderId = 100,
                    RestaurantId = 1,
                    RestaurantName = "Pizza Palace",
                    UserId = 10,
                    Status = OrderStatus.placed.ToString(),
                    Amount = 500,
                    CreatedAt = new DateTime(2026, 8, 18)
                },
                new OwnerOrderDto
                {
                    OrderId = 101,
                    RestaurantId = 2,
                    RestaurantName = "Burger House",
                    UserId = 20,
                    Status = OrderStatus.accepted.ToString(),
                    Amount = 800,
                    CreatedAt = new DateTime(2026, 8, 17)
                }
            };

            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10,
                SortBy = "createdAt",
                SortOrder = "asc"
            };

            _ownerRepositoryMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    1,
                    It.IsAny<OwnerOrderDashboardRequestDto>()))
                .ReturnsAsync((orders, 2));

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Orders retrieved successfully.",
                result.Message);

            var data =
                (OwnerOrderDashboardDto)result.Data;

            Assert.IsNotNull(data);

            Assert.AreEqual(2, data.Orders.Count);
            Assert.AreEqual(1, data.Page);
            Assert.AreEqual(10, data.PageSize);
            Assert.AreEqual(2, data.TotalItems);
            Assert.AreEqual(1, data.TotalPages);

            Assert.AreEqual(100, data.Orders[0].OrderId);
            Assert.AreEqual(1, data.Orders[0].RestaurantId);
            Assert.AreEqual("Pizza Palace", data.Orders[0].RestaurantName);
            Assert.AreEqual(10, data.Orders[0].UserId);
            Assert.AreEqual(
                OrderStatus.placed.ToString(),
                data.Orders[0].Status);
            Assert.AreEqual(500, data.Orders[0].Amount);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    1,
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Once);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_TotalItemsGreaterThanPageSize_CalculatesTotalPages()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 2,
                PageSize = 10
            };

            _ownerRepositoryMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    1,
                    It.IsAny<OwnerOrderDashboardRequestDto>()))
                .ReturnsAsync((
                    new List<OwnerOrderDto>(),
                    25));

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsTrue(result.Success);

            var data =
                (OwnerOrderDashboardDto)result.Data;

            Assert.AreEqual(25, data.TotalItems);
            Assert.AreEqual(3, data.TotalPages);
            Assert.AreEqual(2, data.Page);
            Assert.AreEqual(10, data.PageSize);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_EmptyOrders_ReturnsSuccess()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10
            };

            _ownerRepositoryMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    1,
                    It.IsAny<OwnerOrderDashboardRequestDto>()))
                .ReturnsAsync((
                    new List<OwnerOrderDto>(),
                    0));

            var result =
                await _ownerService.GetOwnerOrdersAsync(1, request);

            Assert.IsTrue(result.Success);

            var data =
                (OwnerOrderDashboardDto)result.Data;

            Assert.IsNotNull(data.Orders);
            Assert.AreEqual(0, data.Orders.Count);
            Assert.AreEqual(0, data.TotalItems);
            Assert.AreEqual(0, data.TotalPages);
        }


        [TestMethod]
        public async Task GetOwnerOrdersAsync_SendsOwnerIdToRepository()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10
            };

            _ownerRepositoryMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    25,
                    It.IsAny<OwnerOrderDashboardRequestDto>()))
                .ReturnsAsync((
                    new List<OwnerOrderDto>(),
                    0));

            await _ownerService.GetOwnerOrdersAsync(25, request);

            _ownerRepositoryMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    25,
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Once);
        }


        // UPDATE ORDER STATUS TESTS

        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_OrderNotFound_ReturnsFailure()
        {
            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync((Order)null);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.accepted.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order not found.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(
                    It.IsAny<long>(),
                    It.IsAny<int>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_UserIsNotRestaurantOwner_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(false);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.accepted.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "You are not an owner of this restaurant.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_PlacedToAccepted_Success()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            _orderRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.accepted.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order status updated successfully.",
                result.Message);

            Assert.AreEqual(
                OrderStatus.accepted.ToString(),
                order.Status);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_AcceptedToDispatched_Success()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.accepted.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            _orderRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.dispatched.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order status updated successfully.",
                result.Message);

            Assert.AreEqual(
                OrderStatus.dispatched.ToString(),
                order.Status);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_DispatchedToDelivered_Success()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.dispatched.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            _orderRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.delivered.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order status updated successfully.",
                result.Message);

            Assert.AreEqual(
                OrderStatus.delivered.ToString(),
                order.Status);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }


        // INVALID TRANSITIONS

        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_AcceptedFromDispatched_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.dispatched.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.accepted.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_DispatchedFromPlaced_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.dispatched.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_DeliveredFromAccepted_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.accepted.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.delivered.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_CancelledFromPlaced_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.cancelled.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_InvalidStatus_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = "something_invalid"
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        // REJECTED

        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_PlacedToRejected_CallsCancelOrderService()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.placed.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var cancelResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Order rejected successfully.",
                Data = null
            };

            _orderServiceMock
                .Setup(x => x.CancelOrderInternalAsync(
                    100,
                    10,
                    UserRole.admin.ToString()))
                .ReturnsAsync(cancelResponse);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.rejected.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order rejected successfully.",
                result.Message);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    100,
                    10,
                    UserRole.admin.ToString()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_AcceptedToRejected_CallsCancelOrderService()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.accepted.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var cancelResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Order rejected successfully.",
                Data = null
            };

            _orderServiceMock
                .Setup(x => x.CancelOrderInternalAsync(
                    100,
                    10,
                    UserRole.admin.ToString()))
                .ReturnsAsync(cancelResponse);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.rejected.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order rejected successfully.",
                result.Message);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    100,
                    10,
                    UserRole.admin.ToString()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_DispatchedToRejected_CallsCancelOrderService()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.dispatched.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var cancelResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Order rejected successfully.",
                Data = null
            };

            _orderServiceMock
                .Setup(x => x.CancelOrderInternalAsync(
                    100,
                    10,
                    UserRole.admin.ToString()))
                .ReturnsAsync(cancelResponse);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.rejected.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Order rejected successfully.",
                result.Message);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    100,
                    10,
                    UserRole.admin.ToString()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_RejectedWhenAlreadyRejected_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.rejected.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.rejected.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_RejectedWhenDelivered_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.delivered.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.rejected.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatusInternalAsync_RejectedWhenCancelled_ReturnsFailure()
        {
            var order = new Order
            {
                Id = 100,
                RestaurantId = 1,
                Status = OrderStatus.cancelled.ToString()
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForUpdateAsync(100))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = OrderStatus.rejected.ToString()
            };

            var result =
                await _ownerService.UpdateOrderStatusInternalAsync(
                    request,
                    10);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                result.Message);

            _orderServiceMock.Verify(
                x => x.CancelOrderInternalAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()),
                Times.Never);
        }
    }
}
