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

        // UPDATE ORDER STATUS TESTS

        [TestMethod]
        public async Task UpdateOrderStatus_Success_ReturnsOk()
        {
            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = "accepted"
            };

            var serviceResponse =
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Order status updated successfully.",
                    Data = new
                    {
                        OrderId = 100,
                        Status = "accepted"
                    }
                };

            _ownerServiceMock
                .Setup(x => x.UpdateOrderStatusAsync(
                    request,
                    1))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.UpdateOrderStatus(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(okResult.Content);

            Assert.IsTrue(okResult.Content.Success);

            Assert.AreEqual(
                "Order status updated successfully.",
                okResult.Content.Message);

            _ownerServiceMock.Verify(
                x => x.UpdateOrderStatusAsync(
                    request,
                    1),
                Times.Once);
        }


        [TestMethod]
        public async Task UpdateOrderStatus_ServiceFailure_ReturnsBadRequest()
        {
            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = "accepted"
            };

            var serviceResponse =
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };

            _ownerServiceMock
                .Setup(x => x.UpdateOrderStatusAsync(
                    request,
                    1))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.UpdateOrderStatus(request);

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
                "Order not found.",
                badRequestResult.Content.Message);

            _ownerServiceMock.Verify(
                x => x.UpdateOrderStatusAsync(
                    request,
                    1),
                Times.Once);
        }


        [TestMethod]
        public async Task UpdateOrderStatus_NoUserIdClaim_ReturnsUnauthorized()
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
                new UpdateOrderStatusRequestDto
                {
                    OrderId = 100,
                    Status = "accepted"
                };

            var result =
                await _controller.UpdateOrderStatus(request);

            Assert.IsInstanceOfType(
                result,
                typeof(UnauthorizedResult));

            _ownerServiceMock.Verify(
                x => x.UpdateOrderStatusAsync(
                    It.IsAny<UpdateOrderStatusRequestDto>(),
                    It.IsAny<long>()),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdateOrderStatus_PassesAuthenticatedUserIdToService()
        {
            var request =
                new UpdateOrderStatusRequestDto
                {
                    OrderId = 100,
                    Status = "dispatched"
                };

            var serviceResponse =
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Order status updated successfully.",
                    Data = null
                };

            _ownerServiceMock
                .Setup(x => x.UpdateOrderStatusAsync(
                    request,
                    25))
                .ReturnsAsync(serviceResponse);

            await SetUserIdAndUpdateOrderStatus(
                request,
                25);

            _ownerServiceMock.Verify(
                x => x.UpdateOrderStatusAsync(
                    request,
                    25),
                Times.Once);
        }


        [TestMethod]
        public async Task UpdateOrderStatus_RejectedStatus_ServiceFailure_ReturnsBadRequest()
        {
            var request = new UpdateOrderStatusRequestDto
            {
                OrderId = 100,
                Status = "rejected"
            };

            var serviceResponse =
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order status cannot be changed.",
                    Data = null
                };

            _ownerServiceMock
                .Setup(x => x.UpdateOrderStatusAsync(
                    request,
                    1))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.UpdateOrderStatus(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsFalse(
                badRequestResult.Content.Success);

            Assert.AreEqual(
                "Order status cannot be changed.",
                badRequestResult.Content.Message);

            _ownerServiceMock.Verify(
                x => x.UpdateOrderStatusAsync(
                    request,
                    1),
                Times.Once);
        }

        private async Task SetUserIdAndUpdateOrderStatus(
            UpdateOrderStatusRequestDto request,
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

            await _controller.UpdateOrderStatus(request);
        }
    }
}
