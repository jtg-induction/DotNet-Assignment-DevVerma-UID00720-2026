using Assignment3.Controllers;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Assignment3.Tests.Controllers
{
    [TestClass]
    public class OwnerControllerTests
    {
        private Mock<IOwnerService> _ownerServiceMock;
        private OwnerController _controller;

        [TestInitialize]
        public void Setup()
        {
            _ownerServiceMock =
                new Mock<IOwnerService>();

            _controller =
                new OwnerController(
                    _ownerServiceMock.Object);

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        "1")
                },
                "TestAuthentication");

            _controller.User =
                new ClaimsPrincipal(identity);
        }


        [TestMethod]
        public async Task GetOrders_Success_ReturnsOk()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 1,
                PageSize = 10
            };

            var serviceResponse =
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Orders retrieved successfully.",
                    Data = new OwnerOrderDashboardDto
                    {
                        Orders = new List<OwnerOrderDto>(),
                        Page = 1,
                        PageSize = 10,
                        TotalItems = 0,
                        TotalPages = 0
                    }
                };

            _ownerServiceMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    1,
                    request))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetOrders(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(okResult.Content);

            Assert.IsTrue(okResult.Content.Success);

            Assert.AreEqual(
                "Orders retrieved successfully.",
                okResult.Content.Message);

            _ownerServiceMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    1,
                    request),
                Times.Once);
        }


        [TestMethod]
        public async Task GetOrders_ServiceFailure_ReturnsBadRequest()
        {
            var request = new OwnerOrderDashboardRequestDto
            {
                Page = 0,
                PageSize = 10
            };

            var serviceResponse =
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Page must be greater than 0.",
                    Data = null
                };

            _ownerServiceMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    1,
                    request))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetOrders(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(
                badRequestResult.Content);

            Assert.IsFalse(
                badRequestResult.Content.Success);

            Assert.AreEqual(
                "Page must be greater than 0.",
                badRequestResult.Content.Message);

            _ownerServiceMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    1,
                    request),
                Times.Once);
        }


        [TestMethod]
        public async Task GetOrders_NoUserIdClaim_ReturnsUnauthorized()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.Name,
                        "Test User")
                },
                "TestAuthentication");

            _controller.User =
                new ClaimsPrincipal(identity);

            var request =
                new OwnerOrderDashboardRequestDto();

            var result =
                await _controller.GetOrders(request);

            Assert.IsInstanceOfType(
                result,
                typeof(UnauthorizedResult));

            _ownerServiceMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<OwnerOrderDashboardRequestDto>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetOrders_PassesAuthenticatedUserIdToService()
        {
            var request =
                new OwnerOrderDashboardRequestDto
                {
                    Page = 1,
                    PageSize = 10
                };

            var serviceResponse =
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Orders retrieved successfully.",
                    Data = null
                };

            _ownerServiceMock
                .Setup(x => x.GetOwnerOrdersAsync(
                    25,
                    request))
                .ReturnsAsync(serviceResponse);

            await SetUserIdAndCall(request, 25);

            _ownerServiceMock.Verify(
                x => x.GetOwnerOrdersAsync(
                    25,
                    request),
                Times.Once);
        }


        private async Task SetUserIdAndCall(
            OwnerOrderDashboardRequestDto request,
            long userId)
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.ToString())
                },
                "TestAuthentication");

            _controller.User =
                new ClaimsPrincipal(identity);

            await _controller.GetOrders(request);
        }
    }
}
