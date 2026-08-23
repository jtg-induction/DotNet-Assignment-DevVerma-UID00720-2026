using Assignment3.DTOs;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Implementations;
using Assignment3.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment3.Tests.Services
{
    [TestClass]
    public class ReportingServiceTests
    {
        private Mock<IReportingRepository> _reportingRepositoryMock;
        private Mock<IRestaurantRepository> _restaurantRepositoryMock;
        private Mock<IReportRenderer> _reportRendererMock;

        private ReportingService _service;

        [TestInitialize]
        public void Setup()
        {
            _reportingRepositoryMock =
                new Mock<IReportingRepository>();

            _restaurantRepositoryMock =
                new Mock<IRestaurantRepository>();

            _reportRendererMock =
                new Mock<IReportRenderer>();

            _service = new ReportingService(
                _reportingRepositoryMock.Object,
                _restaurantRepositoryMock.Object,
                _reportRendererMock.Object);
        }

        // Tests successful Top 10 Ordered Items report generation
        [TestMethod]
        public async Task GetTopOrderedItems_ReturnsSuccess_WhenRestaurantBelongsToAdmin()
        {
            long adminId = 1;

            var request = new TopOrderedItemsRequestDto
            {
                RestaurantId = 5,
                ExcludeMenuItemIds = null
            };

            var data = new List<TopOrderedItemDto>
            {
                new TopOrderedItemDto
                {
                    MenuItemId = 1,
                    MenuItemName = "Burger",
                    RestaurantId = 5,
                    RestaurantName = "Food House",
                    OrderCount = 20
                },
                new TopOrderedItemDto
                {
                    MenuItemId = 2,
                    MenuItemName = "Pizza",
                    RestaurantId = 5,
                    RestaurantName = "Food House",
                    OrderCount = 15
                }
            };

            var pdfBytes = new byte[]
            {
                1, 2, 3, 4
            };

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(
                    adminId,
                    5))
                .ReturnsAsync(true);

            _reportingRepositoryMock
                .Setup(x => x.GetTopOrderedItemsAsync(
                    adminId,
                    request))
                .ReturnsAsync(data);

            _reportRendererMock
                .Setup(x => x.RenderReport(
                    "~/Reports/TopOrderedItems.trdp",
                    data))
                .Returns(pdfBytes);

            var response =
                await _service.GetTopOrderedItemsAsync(
                    adminId,
                    request);

            Assert.IsTrue(response.Success);

            Assert.AreEqual(
                "Top ordered items report generated successfully.",
                response.Message);

            CollectionAssert.AreEqual(
                pdfBytes,
                response.Data);

            _restaurantRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(
                    adminId,
                    5),
                Times.Once);

            _reportingRepositoryMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    adminId,
                    request),
                Times.Once);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    "~/Reports/TopOrderedItems.trdp",
                    data),
                Times.Once);
        }

        // Tests Top 10 Ordered Items when admin does not own the restaurant
        [TestMethod]
        public async Task GetTopOrderedItems_ReturnsFailure_WhenAdminDoesNotOwnRestaurant()
        {
            long adminId = 1;

            var request = new TopOrderedItemsRequestDto
            {
                RestaurantId = 5
            };

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(
                    adminId,
                    5))
                .ReturnsAsync(false);

            var response =
                await _service.GetTopOrderedItemsAsync(
                    adminId,
                    request);

            Assert.IsFalse(response.Success);

            Assert.AreEqual(
                "You are not an owner of this restaurant.",
                response.Message);

            Assert.IsNull(response.Data);

            _reportingRepositoryMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    It.IsAny<long>(),
                    It.IsAny<TopOrderedItemsRequestDto>()),
                Times.Never);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        // Tests Top 10 Ordered Items when restaurant ID is not provided
        [TestMethod]
        public async Task GetTopOrderedItems_DoesNotCheckSpecificRestaurant_WhenRestaurantIdIsNull()
        {
            long adminId = 1;

            var request = new TopOrderedItemsRequestDto
            {
                RestaurantId = null,
                ExcludeMenuItemIds = null
            };

            var data = new List<TopOrderedItemDto>();

            var pdfBytes = new byte[]
            {
                1, 2
            };

            _reportingRepositoryMock
                .Setup(x => x.GetTopOrderedItemsAsync(
                    adminId,
                    request))
                .ReturnsAsync(data);

            _reportRendererMock
                .Setup(x => x.RenderReport(
                    "~/Reports/TopOrderedItems.trdp",
                    data))
                .Returns(pdfBytes);

            var response =
                await _service.GetTopOrderedItemsAsync(
                    adminId,
                    request);

            Assert.IsTrue(response.Success);

            _restaurantRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(
                    It.IsAny<long>(),
                    It.IsAny<int>()),
                Times.Never);

            _reportingRepositoryMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    adminId,
                    request),
                Times.Once);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    "~/Reports/TopOrderedItems.trdp",
                    data),
                Times.Once);
        }

        // Tests that null request is handled
        [TestMethod]
        public async Task GetTopOrderedItems_CreatesEmptyRequest_WhenRequestIsNull()
        {
            long adminId = 1;

            var data = new List<TopOrderedItemDto>();

            var pdfBytes = new byte[]
            {
                1, 2
            };

            _reportingRepositoryMock
                .Setup(x => x.GetTopOrderedItemsAsync(
                    adminId,
                    It.IsAny<TopOrderedItemsRequestDto>()))
                .ReturnsAsync(data);

            _reportRendererMock
                .Setup(x => x.RenderReport(
                    "~/Reports/TopOrderedItems.trdp",
                    data))
                .Returns(pdfBytes);

            var response =
                await _service.GetTopOrderedItemsAsync(
                    adminId,
                    null);

            Assert.IsTrue(response.Success);

            _reportingRepositoryMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    adminId,
                    It.Is<TopOrderedItemsRequestDto>(
                        r => r != null)),
                Times.Once);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    "~/Reports/TopOrderedItems.trdp",
                    data),
                Times.Once);
        }

        // Tests successful Frequently Bought Together report generation
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_ReturnsSuccess_WhenAdminOwnsRestaurant()
        {
            long adminId = 1;
            int restaurantId = 5;

            var data =
                new List<FrequentlyBoughtTogetherDto>
                {
                    new FrequentlyBoughtTogetherDto
                    {
                        Item1Id = 1,
                        Item1Name = "Burger",
                        Item2Id = 2,
                        Item2Name = "Fries",
                        OrderCount = 10
                    },
                    new FrequentlyBoughtTogetherDto
                    {
                        Item1Id = 1,
                        Item1Name = "Burger",
                        Item2Id = 3,
                        Item2Name = "Coke",
                        OrderCount = 8
                    }
                };

            var pdfBytes = new byte[]
            {
                1, 2, 3, 4
            };

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(
                    adminId,
                    restaurantId))
                .ReturnsAsync(true);

            _reportingRepositoryMock
                .Setup(x => x.GetFrequentlyBoughtTogetherAsync(
                    restaurantId))
                .ReturnsAsync(data);

            _reportRendererMock
                .Setup(x => x.RenderReport(
                    "~/Reports/FrequentlyBoughtTogether.trdp",
                    data))
                .Returns(pdfBytes);

            var response =
                await _service.GetFrequentlyBoughtTogetherAsync(
                    adminId,
                    restaurantId);

            Assert.IsTrue(response.Success);

            Assert.AreEqual(
                "Frequently bought together report generated successfully.",
                response.Message);

            CollectionAssert.AreEqual(
                pdfBytes,
                response.Data);

            _restaurantRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(
                    adminId,
                    restaurantId),
                Times.Once);

            _reportingRepositoryMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    restaurantId),
                Times.Once);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    "~/Reports/FrequentlyBoughtTogether.trdp",
                    data),
                Times.Once);
        }

        // Tests Frequently Bought Together when admin does not own restaurant
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_ReturnsFailure_WhenAdminDoesNotOwnRestaurant()
        {
            long adminId = 1;
            int restaurantId = 5;

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(
                    adminId,
                    restaurantId))
                .ReturnsAsync(false);

            var response =
                await _service.GetFrequentlyBoughtTogetherAsync(
                    adminId,
                    restaurantId);

            Assert.IsFalse(response.Success);

            Assert.AreEqual(
                "You are not an owner of this restaurant.",
                response.Message);

            Assert.IsNull(response.Data);

            _reportingRepositoryMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    It.IsAny<int>()),
                Times.Never);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        // Tests Frequently Bought Together when no pairs are found
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_ReturnsSuccess_WhenNoPairsAreFound()
        {
            long adminId = 1;
            int restaurantId = 5;

            var data =
                new List<FrequentlyBoughtTogetherDto>();

            var pdfBytes = new byte[]
            {
                1, 2
            };

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(
                    adminId,
                    restaurantId))
                .ReturnsAsync(true);

            _reportingRepositoryMock
                .Setup(x => x.GetFrequentlyBoughtTogetherAsync(
                    restaurantId))
                .ReturnsAsync(data);

            _reportRendererMock
                .Setup(x => x.RenderReport(
                    "~/Reports/FrequentlyBoughtTogether.trdp",
                    data))
                .Returns(pdfBytes);

            var response =
                await _service.GetFrequentlyBoughtTogetherAsync(
                    adminId,
                    restaurantId);

            Assert.IsTrue(response.Success);

            CollectionAssert.AreEqual(
                pdfBytes,
                response.Data);

            _reportingRepositoryMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    restaurantId),
                Times.Once);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    "~/Reports/FrequentlyBoughtTogether.trdp",
                    data),
                Times.Once);
        }

        // Tests that the correct restaurant ID is used throughout the service
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_UsesCorrectRestaurantId()
        {
            long adminId = 25;
            int restaurantId = 100;

            var data =
                new List<FrequentlyBoughtTogetherDto>();

            var pdfBytes = new byte[]
            {
                1
            };

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(
                    25,
                    100))
                .ReturnsAsync(true);

            _reportingRepositoryMock
                .Setup(x => x.GetFrequentlyBoughtTogetherAsync(
                    100))
                .ReturnsAsync(data);

            _reportRendererMock
                .Setup(x => x.RenderReport(
                    "~/Reports/FrequentlyBoughtTogether.trdp",
                    data))
                .Returns(pdfBytes);

            await _service.GetFrequentlyBoughtTogetherAsync(
                adminId,
                restaurantId);

            _restaurantRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(
                    25,
                    100),
                Times.Once);

            _reportingRepositoryMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    100),
                Times.Once);

            _reportRendererMock.Verify(
                x => x.RenderReport(
                    "~/Reports/FrequentlyBoughtTogether.trdp",
                    data),
                Times.Once);
        }
    }
}
