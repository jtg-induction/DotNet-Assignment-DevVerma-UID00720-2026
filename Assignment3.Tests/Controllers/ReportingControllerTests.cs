using Assignment3.Controllers;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Assignment3.Tests.Controllers
{
    [TestClass]
    public class ReportingControllerTests
    {
        private Mock<IReportingService> _reportingServiceMock;
        private ReportingController _controller;

        [TestInitialize]
        public void Setup()
        {
            _reportingServiceMock = new Mock<IReportingService>();

            _controller = new ReportingController(
                _reportingServiceMock.Object);

            _controller.Request = new HttpRequestMessage();

            _controller.Configuration = new HttpConfiguration();
        }

        private void SetUser(long userId)
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.ToString())
                },
                "TestAuthentication");

            _controller.User = new ClaimsPrincipal(identity);
        }

        // Tests successful Top 10 Ordered Items report generation
        [TestMethod]
        public async Task GetTopOrderedItems_ReturnsPdf_WhenRequestIsSuccessful()
        {
            SetUser(1);

            var request = new TopOrderedItemsRequestDto
            {
                RestaurantId = 5
            };

            var pdfBytes = new byte[]
            {
                1, 2, 3, 4
            };

            _reportingServiceMock
                .Setup(x => x.GetTopOrderedItemsAsync(
                    1,
                    request))
                .ReturnsAsync(new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "Top ordered items report generated successfully.",
                    Data = pdfBytes
                });

            var result = await _controller.GetTopOrderedItems(request);

            var responseMessage = result as ResponseMessageResult;

            Assert.IsNotNull(responseMessage);

            Assert.AreEqual(
                HttpStatusCode.OK,
                responseMessage.Response.StatusCode);

            Assert.AreEqual(
                "application/pdf",
                responseMessage.Response.Content.Headers.ContentType.MediaType);

            var returnedBytes =
                await responseMessage.Response.Content.ReadAsByteArrayAsync();

            CollectionAssert.AreEqual(
                pdfBytes,
                returnedBytes);

            _reportingServiceMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    1,
                    request),
                Times.Once);
        }

        // Tests Top 10 Ordered Items when authentication claim is missing
        [TestMethod]
        public async Task GetTopOrderedItems_ReturnsUnauthorized_WhenUserIdClaimIsMissing()
        {
            var identity = new ClaimsIdentity(
                authenticationType: "TestAuthentication");

            _controller.User = new ClaimsPrincipal(identity);

            var request = new TopOrderedItemsRequestDto
            {
                RestaurantId = 5
            };

            var result =
                await _controller.GetTopOrderedItems(request);

            Assert.IsInstanceOfType(
                result,
                typeof(UnauthorizedResult));

            _reportingServiceMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    It.IsAny<long>(),
                    It.IsAny<TopOrderedItemsRequestDto>()),
                Times.Never);
        }

        // Tests Top 10 Ordered Items when service returns failure
        [TestMethod]
        public async Task GetTopOrderedItems_ReturnsBadRequest_WhenServiceFails()
        {
            SetUser(1);

            var request = new TopOrderedItemsRequestDto
            {
                RestaurantId = 5
            };

            _reportingServiceMock
                .Setup(x => x.GetTopOrderedItemsAsync(
                    1,
                    request))
                .ReturnsAsync(new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "You are not an owner of this restaurant.",
                    Data = null
                });

            var result =
                await _controller.GetTopOrderedItems(request);

            var badRequest =
                result as BadRequestErrorMessageResult;

            Assert.IsNotNull(badRequest);

            Assert.AreEqual(
                "You are not an owner of this restaurant.",
                badRequest.Message);

            _reportingServiceMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    1,
                    request),
                Times.Once);
        }

        // Tests that the Top 10 Ordered Items request is passed correctly
        [TestMethod]
        public async Task GetTopOrderedItems_PassesCorrectRequestToService()
        {
            SetUser(10);

            var request = new TopOrderedItemsRequestDto
            {
                RestaurantId = 25,
                ExcludeMenuItemIds = null
            };

            _reportingServiceMock
                .Setup(x => x.GetTopOrderedItemsAsync(
                    10,
                    request))
                .ReturnsAsync(new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "Success",
                    Data = new byte[] { 1 }
                });

            await _controller.GetTopOrderedItems(request);

            _reportingServiceMock.Verify(
                x => x.GetTopOrderedItemsAsync(
                    10,
                    request),
                Times.Once);
        }

        // Tests successful Frequently Bought Together report generation
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_ReturnsPdf_WhenRequestIsSuccessful()
        {
            SetUser(1);

            int restaurantId = 5;

            var pdfBytes = new byte[]
            {
                1, 2, 3, 4
            };

            _reportingServiceMock
                .Setup(x => x.GetFrequentlyBoughtTogetherAsync(
                    1,
                    restaurantId))
                .ReturnsAsync(new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "Frequently bought together report generated successfully.",
                    Data = pdfBytes
                });

            var result =
                await _controller.GetFrequentlyBoughtTogether(
                    restaurantId);

            var responseMessage =
                result as ResponseMessageResult;

            Assert.IsNotNull(responseMessage);

            Assert.AreEqual(
                HttpStatusCode.OK,
                responseMessage.Response.StatusCode);

            Assert.AreEqual(
                "application/pdf",
                responseMessage.Response.Content.Headers.ContentType.MediaType);

            var returnedBytes =
                await responseMessage.Response.Content.ReadAsByteArrayAsync();

            CollectionAssert.AreEqual(
                pdfBytes,
                returnedBytes);

            _reportingServiceMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    1,
                    restaurantId),
                Times.Once);
        }

        // Tests Frequently Bought Together when authentication claim is missing
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_ReturnsUnauthorized_WhenUserIdClaimIsMissing()
        {
            var identity = new ClaimsIdentity(
                authenticationType: "TestAuthentication");

            _controller.User = new ClaimsPrincipal(identity);

            var result =
                await _controller.GetFrequentlyBoughtTogether(5);

            Assert.IsInstanceOfType(
                result,
                typeof(UnauthorizedResult));

            _reportingServiceMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    It.IsAny<long>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        // Tests Frequently Bought Together when service returns failure
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_ReturnsBadRequest_WhenServiceFails()
        {
            SetUser(1);

            int restaurantId = 5;

            _reportingServiceMock
                .Setup(x => x.GetFrequentlyBoughtTogetherAsync(
                    1,
                    restaurantId))
                .ReturnsAsync(new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "You are not an owner of this restaurant.",
                    Data = null
                });

            var result =
                await _controller.GetFrequentlyBoughtTogether(
                    restaurantId);

            var badRequest =
                result as BadRequestErrorMessageResult;

            Assert.IsNotNull(badRequest);

            Assert.AreEqual(
                "You are not an owner of this restaurant.",
                badRequest.Message);

            _reportingServiceMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    1,
                    restaurantId),
                Times.Once);
        }

        // Tests that the correct restaurant ID is passed to the service
        [TestMethod]
        public async Task GetFrequentlyBoughtTogether_PassesCorrectRestaurantId()
        {
            SetUser(25);

            int restaurantId = 100;

            _reportingServiceMock
                .Setup(x => x.GetFrequentlyBoughtTogetherAsync(
                    25,
                    100))
                .ReturnsAsync(new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "Success",
                    Data = new byte[] { 1 }
                });

            await _controller.GetFrequentlyBoughtTogether(
                restaurantId);

            _reportingServiceMock.Verify(
                x => x.GetFrequentlyBoughtTogetherAsync(
                    25,
                    100),
                Times.Once);
        }
    }
}
