using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.Enums;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment3.Tests.Services
{
    [TestClass]
    public class SuperAdminServiceTests
    {
        private Mock<FoodOrderingContext> _contextMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IRestaurantRepository> _restaurantRepositoryMock;

        private SuperAdminService _superAdminService;

        [TestInitialize]
        public void Setup()
        {
            _contextMock =
                new Mock<FoodOrderingContext>();

            _userRepositoryMock =
                new Mock<IUserRepository>();

            _restaurantRepositoryMock =
                new Mock<IRestaurantRepository>();

            _superAdminService =
                new SuperAdminService(
                    _contextMock.Object,
                    _userRepositoryMock.Object,
                    _restaurantRepositoryMock.Object);
        }


        // ============================================================
        // ADD RESTAURANT OWNER TESTS
        // ============================================================

        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_RestaurantDoesNotExist_ReturnsFailure()
        {
            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync((Restaurant)null);

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

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Restaurant not found.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.GetUserByEmailAsync(It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_InactiveRestaurant_ReturnsFailure()
        {
            var restaurant = new Restaurant
            {
                Id = 1,
                Name = "Pizza Palace",
                Email = "pizza@example.com",
                IsActive = false
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync(restaurant);

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

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Restaurant is inactive.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.GetUserByEmailAsync(It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_NoOwners_ReturnsFailure()
        {
            var restaurant = new Restaurant
            {
                Id = 1,
                Name = "Pizza Palace",
                Email = "pizza@example.com",
                IsActive = true
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync(restaurant);

            var request = new AddRestaurantOwnerRequestDto
            {
                RestaurantId = 1,
                Owners = new List<RestaurantOwnerDto>()
            };

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "At least one restaurant owner is required.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.GetUserByEmailAsync(It.IsAny<string>()),
                Times.Never);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.IsAny<RestaurantOwner>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_RestaurantAndOwnerEmailSame_ReturnsFailure()
        {
            var restaurant = new Restaurant
            {
                Id = 1,
                Name = "Pizza Palace",
                Email = "pizza@example.com",
                IsActive = true
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync(restaurant);

            var request = new AddRestaurantOwnerRequestDto
            {
                RestaurantId = 1,
                Owners = new List<RestaurantOwnerDto>
                {
                    new RestaurantOwnerDto
                    {
                        OwnerName = "Pizza Palace Owner",
                        OwnerEmail = "pizza@example.com"
                    }
                }
            };

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Restaurant email and owner email must be different.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.GetUserByEmailAsync(It.IsAny<string>()),
                Times.Never);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.IsAny<RestaurantOwner>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_UserAlreadyOwner_ReturnsFailure()
        {
            var restaurant = new Restaurant
            {
                Id = 1,
                Name = "Pizza Palace",
                Email = "pizza@example.com",
                IsActive = true
            };

            var existingUser = new User
            {
                Id = 10,
                Name = "Rahul Sharma",
                Email = "rahul@example.com",
                Role = "customer"
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync(restaurant);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync("rahul@example.com"))
                .ReturnsAsync(existingUser);

            _userRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(true);

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

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "User is already an owner of this restaurant.",
                result.Message);

            Assert.AreEqual(
                UserRole.admin.ToString(),
                existingUser.Role);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.IsAny<RestaurantOwner>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_ExistingUser_UpgradesRoleAndCreatesLink()
        {
            var restaurant = new Restaurant
            {
                Id = 1,
                Name = "Pizza Palace",
                Email = "pizza@example.com",
                IsActive = true
            };

            var existingUser = new User
            {
                Id = 10,
                Name = "Rahul Sharma",
                Email = "rahul@example.com",
                Role = UserRole.customer.ToString(),
                IsActive = true,
                Balance = 500
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync(restaurant);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(
                    "rahul@example.com"))
                .ReturnsAsync(existingUser);

            _userRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(false);

            _restaurantRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

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

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Restaurant owners onboarded successfully.",
                result.Message);

            Assert.AreEqual(
                UserRole.admin.ToString(),
                existingUser.Role);

            _userRepositoryMock.Verify(
                x => x.AddUser(
                    It.IsAny<User>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(10, 1),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.Is<RestaurantOwner>(ro =>
                        ro.UserId == 10 &&
                        ro.RestaurantId == 1)),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_NewUser_CreatesOwnerAndLink()
        {
            var restaurant = new Restaurant
            {
                Id = 1,
                Name = "Pizza Palace",
                Email = "pizza@example.com",
                IsActive = true
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync(restaurant);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(
                    "rahul@example.com"))
                .ReturnsAsync((User)null);

            _userRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(false);

            _restaurantRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.AddUser(
                    It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    user.Id = 10;
                });

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

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Restaurant owners onboarded successfully.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.AddUser(
                    It.Is<User>(u =>
                        u.Name == "Rahul Sharma" &&
                        u.Email == "rahul@example.com" &&
                        u.Role == UserRole.admin.ToString() &&
                        u.IsActive &&
                        u.Balance == 0 &&
                        u.Password != "123456789")),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(10, 1),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.Is<RestaurantOwner>(ro =>
                        ro.UserId == 10 &&
                        ro.RestaurantId == 1)),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }


        // ============================================================
        // ADD RESTAURANT TESTS
        // ============================================================

        [TestMethod]
        public async Task AddRestaurantInternalAsync_RestaurantEmailAlreadyExists_ReturnsFailure()
        {
            var existingRestaurant = new Restaurant
            {
                Id = 1,
                Name = "Existing Restaurant",
                Email = "pizza@example.com"
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByEmailAsync(
                    "pizza@example.com"))
                .ReturnsAsync(existingRestaurant);

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

            var result =
                await _superAdminService
                    .AddRestaurantInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Restaurant with this email already exists.",
                result.Message);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurant(
                    It.IsAny<Restaurant>()),
                Times.Never);

            _restaurantRepositoryMock.Verify(
                x => x.AddAddress(
                    It.IsAny<Address>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantInternalAsync_NoOwners_ReturnsFailure()
        {
            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByEmailAsync(
                    "pizza@example.com"))
                .ReturnsAsync((Restaurant)null);

            _restaurantRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

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

                Owners = new List<RestaurantOwnerDto>()
            };

            var result =
                await _superAdminService
                    .AddRestaurantInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "At least one restaurant owner is required.",
                result.Message);

            _restaurantRepositoryMock.Verify(
                x => x.AddAddress(
                    It.IsAny<Address>()),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurant(
                    It.IsAny<Restaurant>()),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.IsAny<RestaurantOwner>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantInternalAsync_Success_CreatesRestaurantAddressAndOwner()
        {
            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByEmailAsync(
                    "pizza@example.com"))
                .ReturnsAsync((Restaurant)null);

            _restaurantRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(
                    "rahul@example.com"))
                .ReturnsAsync((User)null);

            _userRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _restaurantRepositoryMock
                .Setup(x => x.AddRestaurant(
                    It.IsAny<Restaurant>()))
                .Callback<Restaurant>(restaurant =>
                {
                    restaurant.Id = 1;
                });

            _restaurantRepositoryMock
                .Setup(x => x.AddAddress(
                    It.IsAny<Address>()))
                .Callback<Address>(address =>
                {
                    address.Id = 20;
                });

            _userRepositoryMock
                .Setup(x => x.AddUser(
                    It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    user.Id = 10;
                });

            _restaurantRepositoryMock
                .Setup(x => x.IsRestaurantOwnerAsync(10, 1))
                .ReturnsAsync(false);

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

            var result =
                await _superAdminService
                    .AddRestaurantInternalAsync(request);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Restaurant and owners onboarded successfully.",
                result.Message);

            _restaurantRepositoryMock.Verify(
                x => x.AddAddress(
                    It.Is<Address>(a =>
                        a.BuildingNumber == "10" &&
                        a.Locality == "Sector 18" &&
                        a.City == "Noida" &&
                        a.State == "Uttar Pradesh" &&
                        a.Country == "India" &&
                        a.PostalCode == "201301" &&
                        a.UserId == null)),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurant(
                    It.Is<Restaurant>(r =>
                        r.Name == "Pizza Palace" &&
                        r.Email == "pizza@example.com" &&
                        r.IsActive &&
                        r.AddressId == 20)),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUser(
                    It.Is<User>(u =>
                        u.Name == "Rahul Sharma" &&
                        u.Email == "rahul@example.com" &&
                        u.Role == UserRole.admin.ToString() &&
                        u.IsActive &&
                        u.Balance == 0 &&
                        u.Password != "123456789")),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.IsRestaurantOwnerAsync(10, 1),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.Is<RestaurantOwner>(ro =>
                        ro.UserId == 10 &&
                        ro.RestaurantId == 1)),
                Times.Once);
        }
    }
}
