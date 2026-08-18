using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Enums;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Implementations;
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
        private Mock<IOwnerRepository> _ownerRepositoryMock;
        private OwnerService _ownerService;

        [TestInitialize]
        public void Setup()
        {
            _ownerRepositoryMock =
                new Mock<IOwnerRepository>();

            _ownerService =
                new OwnerService(
                    _ownerRepositoryMock.Object);
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
    }
}
