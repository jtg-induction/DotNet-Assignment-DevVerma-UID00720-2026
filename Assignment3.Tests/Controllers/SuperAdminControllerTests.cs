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
    public class SuperAdminControllerTests
    {
        private Mock<ISuperAdminService> _superAdminServiceMock;
        private SuperAdminController _controller;

        [TestInitialize]
        public void Setup()
        {
            _superAdminServiceMock =
                new Mock<ISuperAdminService>();

            _controller =
                new SuperAdminController(
                    _superAdminServiceMock.Object);
        }


        // ADD RESTAURANT TESTS

        [TestMethod]
        public async Task AddRestaurant_Success_ReturnsOk()
        {
            var request = new AddRestaurantRequestDto
            {
                RestaurantName = "Pizza Palace",
                RestaurantEmail = "pizza@example.com",
                BuildingNumber = "10",
                Locality = "Sector 18",
                City = "Noida",
                State = "Uttar Pradesh",
                Country = "India",
                PostalCode = "201301",

                Owners = new List<RestaurantOwnerDto>
                {
                    new RestaurantOwnerDto
                    {
                        OwnerName = "Rahul Sharma",
                        OwnerEmail = "rahul@example.com"
                    }
                }
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message =
                    "Restaurant and owners onboarded successfully.",
                Data = new
                {
                    RestaurantId = 1
                }
            };

            _superAdminServiceMock
                .Setup(x => x.AddRestaurantAsync(request))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.AddRestaurant(request);

            Assert.IsInstanceOfType(
                result,
                typeof(
                    OkNegotiatedContentResult<
                        ApiResponse<object>>));

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(okResult.Content);

            Assert.IsTrue(
                okResult.Content.Success);

            Assert.AreEqual(
                "Restaurant and owners onboarded successfully.",
                okResult.Content.Message);

            _superAdminServiceMock.Verify(
                x => x.AddRestaurantAsync(request),
                Times.Once);
        }


        [TestMethod]
        public async Task AddRestaurant_Failure_ReturnsBadRequest()
        {
            var request = new AddRestaurantRequestDto
            {
                RestaurantName = "Pizza Palace",
                RestaurantEmail = "pizza@example.com",
                BuildingNumber = "10",
                Locality = "Sector 18",
                City = "Noida",
                State = "Uttar Pradesh",
                Country = "India",
                PostalCode = "201301",

                Owners = new List<RestaurantOwnerDto>
                {
                    new RestaurantOwnerDto
                    {
                        OwnerName = "Rahul Sharma",
                        OwnerEmail = "rahul@example.com"
                    }
                }
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = false,
                Message =
                    "Restaurant with this email already exists.",
                Data = null
            };

            _superAdminServiceMock
                .Setup(x => x.AddRestaurantAsync(request))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.AddRestaurant(request);

            Assert.IsInstanceOfType(
                result,
                typeof(
                    NegotiatedContentResult<
                        ApiResponse<object>>));

            var badRequest =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequest.StatusCode);

            Assert.IsFalse(
                badRequest.Content.Success);

            Assert.AreEqual(
                "Restaurant with this email already exists.",
                badRequest.Content.Message);

            _superAdminServiceMock.Verify(
                x => x.AddRestaurantAsync(request),
                Times.Once);
        }


        // ADD RESTAURANT OWNER TESTS

        [TestMethod]
        public async Task AddRestaurantOwner_Success_ReturnsOk()
        {
            var request = new AddRestaurantOwnerRequestDto
            {
                RestaurantId = 1,

                Owners = new List<RestaurantOwnerDto>
                {
                    new RestaurantOwnerDto
                    {
                        OwnerName = "Rahul Sharma",
                        OwnerEmail = "rahul@example.com"
                    }
                }
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message =
                    "Restaurant owners onboarded successfully.",
                Data = new
                {
                    RestaurantId = 1
                }
            };

            _superAdminServiceMock
                .Setup(x => x.AddRestaurantOwnerAsync(request))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.AddRestaurantOwner(request);

            Assert.IsInstanceOfType(
                result,
                typeof(
                    OkNegotiatedContentResult<
                        ApiResponse<object>>));

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            Assert.IsNotNull(
                okResult.Content);

            Assert.IsTrue(
                okResult.Content.Success);

            Assert.AreEqual(
                "Restaurant owners onboarded successfully.",
                okResult.Content.Message);

            _superAdminServiceMock.Verify(
                x => x.AddRestaurantOwnerAsync(request),
                Times.Once);
        }


        [TestMethod]
        public async Task AddRestaurantOwner_Failure_ReturnsBadRequest()
        {
            var request = new AddRestaurantOwnerRequestDto
            {
                RestaurantId = 1,

                Owners = new List<RestaurantOwnerDto>
                {
                    new RestaurantOwnerDto
                    {
                        OwnerName = "Rahul Sharma",
                        OwnerEmail = "rahul@example.com"
                    }
                }
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "Restaurant not found.",
                Data = null
            };

            _superAdminServiceMock
                .Setup(x => x.AddRestaurantOwnerAsync(request))
                .ReturnsAsync(serviceResponse);

            var result =
                await _controller.AddRestaurantOwner(request);

            Assert.IsInstanceOfType(
                result,
                typeof(
                    NegotiatedContentResult<
                        ApiResponse<object>>));

            var badRequest =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                badRequest.StatusCode);

            Assert.IsFalse(
                badRequest.Content.Success);

            Assert.AreEqual(
                "Restaurant not found.",
                badRequest.Content.Message);

            _superAdminServiceMock.Verify(
                x => x.AddRestaurantOwnerAsync(request),
                Times.Once);
        }
    }
}
