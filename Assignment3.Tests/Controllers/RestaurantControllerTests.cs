using Assignment3.Controllers;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Assignment3.Tests.Controllers
{
    [TestClass]
    public class RestaurantControllerTests
    {
        private Mock<IRestaurantService> _restaurantServiceMock;
        private RestaurantsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _restaurantServiceMock =
                new Mock<IRestaurantService>();

            _controller =
                new RestaurantsController(
                    _restaurantServiceMock.Object);
        }


        // GET AVAILABLE RESTAURANTS

        [TestMethod]
        public async Task GetAvailableRestaurants_Success_ReturnsOk()
        {
            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Restaurants retrieved successfully.",
                Data = new
                {
                    Restaurants = new List<RestaurantDto>
                    {
                        new RestaurantDto
                        {
                            Id = 1,
                            Name = "Pizza Palace",
                            Email = "pizza@test.com",
                            AddressId = 10
                        },
                        new RestaurantDto
                        {
                            Id = 2,
                            Name = "Burger House",
                            Email = "burger@test.com",
                            AddressId = 20
                        }
                    },
                    Page = 1,
                    PageSize = 10,
                    TotalItems = 2,
                    TotalPages = 1
                }
            };

            _restaurantServiceMock
                .Setup(x => x.GetAvailableRestaurantsAsync(1, 10))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetAvailableRestaurants(1, 10);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(okResult.Content);

            Assert.IsTrue(okResult.Content.Success);

            Assert.AreEqual(
                "Restaurants retrieved successfully.",
                okResult.Content.Message);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(1, 10),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurants_DefaultParameters_ReturnsOk()
        {
            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Restaurants retrieved successfully.",
                Data = null
            };

            _restaurantServiceMock
                .Setup(x => x.GetAvailableRestaurantsAsync(1, 10))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetAvailableRestaurants();

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(1, 10),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurants_InvalidPage_ReturnsBadRequest()
        {
            var result =
                await _controller.GetAvailableRestaurants(0, 10);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            Assert.IsFalse(
                badRequestResult.Content.Success);

            Assert.AreEqual(
                "Page and pageSize must be greater than 0 and pageSize must be between 1 and 50",
                badRequestResult.Content.Message);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetAvailableRestaurants_NegativePage_ReturnsBadRequest()
        {
            var result =
                await _controller.GetAvailableRestaurants(-1, 10);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            Assert.IsFalse(
                badRequestResult.Content.Success);

            Assert.AreEqual(
                "Page and pageSize must be greater than 0 and pageSize must be between 1 and 50",
                badRequestResult.Content.Message);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetAvailableRestaurants_PageSizeZero_ReturnsBadRequest()
        {
            var result =
                await _controller.GetAvailableRestaurants(1, 0);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            Assert.IsFalse(
                badRequestResult.Content.Success);

            Assert.AreEqual(
                "Page and pageSize must be greater than 0 and pageSize must be between 1 and 50",
                badRequestResult.Content.Message);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetAvailableRestaurants_NegativePageSize_ReturnsBadRequest()
        {
            var result =
                await _controller.GetAvailableRestaurants(1, -5);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            Assert.IsFalse(
                badRequestResult.Content.Success);

            Assert.AreEqual(
                "Page and pageSize must be greater than 0 and pageSize must be between 1 and 50",
                badRequestResult.Content.Message);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetAvailableRestaurants_PageSizeGreaterThan50_ReturnsBadRequest()
        {
            var result =
                await _controller.GetAvailableRestaurants(1, 51);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            Assert.IsFalse(
                badRequestResult.Content.Success);

            Assert.AreEqual(
                "Page and pageSize must be greater than 0 and pageSize must be between 1 and 50",
                badRequestResult.Content.Message);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }


        [TestMethod]
        public async Task GetAvailableRestaurants_PageSize50_ReturnsOk()
        {
            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Restaurants retrieved successfully.",
                Data = null
            };

            _restaurantServiceMock
                .Setup(x => x.GetAvailableRestaurantsAsync(1, 50))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetAvailableRestaurants(1, 50);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _restaurantServiceMock.Verify(
                x => x.GetAvailableRestaurantsAsync(1, 50),
                Times.Once);
        }


        // GET MENU ITEMS

        [TestMethod]
        public async Task GetMenuItems_Success_ReturnsOk()
        {
            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Menu items retrieved successfully.",
                Data = new List<MenuItemDto>
                {
                    new MenuItemDto
                    {
                        Id = 1,
                        RestaurantId = 1,
                        Name = "Margherita Pizza",
                        Price = 250,
                        Description = "Classic cheese pizza",
                        Stock = 10
                    },
                    new MenuItemDto
                    {
                        Id = 2,
                        RestaurantId = 1,
                        Name = "Farmhouse Pizza",
                        Price = 350,
                        Description = "Vegetable pizza",
                        Stock = 5
                    }
                }
            };

            _restaurantServiceMock
                .Setup(x => x.GetAvailableMenuItemsAsync(1))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetMenuItems(1);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(okResult.Content);

            Assert.IsTrue(okResult.Content.Success);

            Assert.AreEqual(
                "Menu items retrieved successfully.",
                okResult.Content.Message);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableMenuItemsAsync(1),
                Times.Once);
        }


        [TestMethod]
        public async Task GetMenuItems_NoItems_ReturnsOk()
        {
            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Menu items retrieved successfully.",
                Data = new List<MenuItemDto>()
            };

            _restaurantServiceMock
                .Setup(x => x.GetAvailableMenuItemsAsync(1))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.GetMenuItems(1);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(okResult.Content);

            Assert.IsTrue(okResult.Content.Success);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableMenuItemsAsync(1),
                Times.Once);
        }


        [TestMethod]
        public async Task GetMenuItems_ValidRestaurantId_CallsServiceOnce()
        {
            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Menu items retrieved successfully.",
                Data = null
            };

            _restaurantServiceMock
                .Setup(x => x.GetAvailableMenuItemsAsync(5))
                .ReturnsAsync(serviceResponse);

            await _controller.GetMenuItems(5);

            _restaurantServiceMock.Verify(
                x => x.GetAvailableMenuItemsAsync(5),
                Times.Once);
        }
    }
}
