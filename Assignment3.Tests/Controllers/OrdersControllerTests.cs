using Assignment3.Controllers;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using Assignment3.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Assignment3.Tests.Controllers
{
    [TestClass]
    public class OrdersControllerTests
    {
        private Mock<IOrderService> _orderServiceMock;
        private OrdersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _orderServiceMock = new Mock<IOrderService>();

            _controller = new OrdersController(
                _orderServiceMock.Object
            );
        }


        // CREATE ORDER

        [TestMethod]
        public async Task CreateOrder_Success_ReturnsOk()
        {
            long userId = 1;

            SetAuthenticatedUser(userId);

            var request = new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Order placed successfully.",
                Data = new
                {
                    OrderId = 100
                }
            };

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(userId, request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.CreateOrder(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _orderServiceMock.Verify(
                x => x.CreateOrderAsync(userId, request),
                Times.Once);
        }


        [TestMethod]
        public async Task CreateOrder_ServiceFails_ReturnsBadRequest()
        {
            long userId = 1;

            SetAuthenticatedUser(userId);

            var request = new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "Insufficient wallet balance.",
                Data = null
            };

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(userId, request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.CreateOrder(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            _orderServiceMock.Verify(
                x => x.CreateOrderAsync(userId, request),
                Times.Once);
        }


        [TestMethod]
        public async Task CreateOrder_NoAuthenticatedUser_ReturnsUnauthorized()
        {
            var request = new CreateOrderRequestDto
            {
                RestaurantId = 1,
                AddressId = 1
            };

            var result = await _controller.CreateOrder(request);

            Assert.IsInstanceOfType(
                result,
                typeof(UnauthorizedResult));

            _orderServiceMock.Verify(
                x => x.CreateOrderAsync(
                    It.IsAny<long>(),
                    It.IsAny<CreateOrderRequestDto>()),
                Times.Never);
        }


        // GET ORDER DETAILS

        [TestMethod]
        public async Task GetOrderDetails_Success_ReturnsOk()
        {
            long userId = 1;
            long orderId = 100;

            SetAuthenticatedUser(userId);

            var orderDetails = new OrderDetailsDto
            {
                OrderId = orderId,
                Status = OrderStatus.placed.ToString(),
                RestaurantName = "Pizza Palace",
                TotalAmount = 400
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Order details retrieved successfully.",
                Data = orderDetails
            };

            _orderServiceMock
                .Setup(x => x.GetOrderDetailsAsync(orderId, userId))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetOrderDetails(orderId);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _orderServiceMock.Verify(
                x => x.GetOrderDetailsAsync(orderId, userId),
                Times.Once);
        }


        [TestMethod]
        public async Task GetOrderDetails_OrderNotFound_ReturnsNotFound()
        {
            long userId = 1;
            long orderId = 100;

            SetAuthenticatedUser(userId);

            var serviceResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "Order not found.",
                Data = null
            };

            _orderServiceMock
                .Setup(x => x.GetOrderDetailsAsync(orderId, userId))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetOrderDetails(orderId);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var notFoundResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.NotFound,
                notFoundResult.StatusCode);

            _orderServiceMock.Verify(
                x => x.GetOrderDetailsAsync(orderId, userId),
                Times.Once);
        }


        [TestMethod]
        public async Task GetOrderDetails_NoAuthenticatedUser_ReturnsUnauthorized()
        {
            long orderId = 100;

            var result =
                await _controller.GetOrderDetails(orderId);

            Assert.IsInstanceOfType(
                result,
                typeof(UnauthorizedResult));

            _orderServiceMock.Verify(
                x => x.GetOrderDetailsAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>()),
                Times.Never);
        }


        // HELPER METHOD

        private void SetAuthenticatedUser(long userId)
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                "TestAuthentication");

            var principal = new ClaimsPrincipal(identity);

            _controller.User = principal;
        }
    }
}
