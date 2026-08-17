using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.Enums;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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


        // ADD RESTAURANT OWNER TESTS


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_RestaurantDoesNotExist_ReturnsFailure()
        {
            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync((Restaurant)null);

            var request = new AddRestaurantOwnerRequestDto
            {
                RestaurantId = 1,
                OwnerName = "Rahul Sharma",
                OwnerEmail = "rahul@example.com",
                OwnerPassword = "Rahul@12345"
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
                OwnerName = "Rahul Sharma",
                OwnerEmail = "rahul@example.com",
                OwnerPassword = "Rahul@12345"
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
                OwnerName = "Pizza Palace Owner",
                OwnerEmail = "pizza@example.com",
                OwnerPassword = "Password@123"
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

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_UserAlreadyExists_ReturnsFailure()
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
                Email = "rahul@example.com"
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByIdAsync(1))
                .ReturnsAsync(restaurant);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync("rahul@example.com"))
                .ReturnsAsync(existingUser);

            var request = new AddRestaurantOwnerRequestDto
            {
                RestaurantId = 1,
                OwnerName = "Rahul Sharma",
                OwnerEmail = "rahul@example.com",
                OwnerPassword = "Rahul@12345"
            };

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "User with this email already exists.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.IsAny<RestaurantOwner>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantOwnerInternalAsync_Success_CreatesOwnerAndLink()
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
                .Setup(x => x.GetUserByEmailAsync("rahul@example.com"))
                .ReturnsAsync((User)null);

            _userRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _restaurantRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    user.Id = 10;
                });

            var request = new AddRestaurantOwnerRequestDto
            {
                RestaurantId = 1,
                OwnerName = "Rahul Sharma",
                OwnerEmail = "rahul@example.com",
                OwnerPassword = "Rahul@12345"
            };

            var result =
                await _superAdminService
                    .AddRestaurantOwnerInternalAsync(request);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Restaurant owner onboarded successfully.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.AddUser(
                    It.Is<User>(u =>
                        u.Name == "Rahul Sharma" &&
                        u.Email == "rahul@example.com" &&
                        u.Role == UserRole.admin.ToString() &&
                        u.IsActive &&
                        u.Balance == 1000 &&
                        u.Password != "Rahul@12345")),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurantOwner(
                    It.Is<RestaurantOwner>(ro =>
                        ro.UserId == 10 &&
                        ro.RestaurantId == 1)),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }


        // ADD RESTAURANT TESTS


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
                OwnerName = "Rahul Sharma",
                OwnerEmail = "rahul@example.com",
                OwnerPassword = "Rahul@12345"
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
        }


        [TestMethod]
        public async Task AddRestaurantInternalAsync_RestaurantAndOwnerEmailSame_ReturnsFailure()
        {
            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByEmailAsync(
                    "pizza@example.com"))
                .ReturnsAsync((Restaurant)null);

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
                OwnerName = "Rahul Sharma",
                OwnerEmail = "pizza@example.com",
                OwnerPassword = "Rahul@12345"
            };

            var result =
                await _superAdminService
                    .AddRestaurantInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "Restaurant email and owner email must be different.",
                result.Message);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurant(
                    It.IsAny<Restaurant>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantInternalAsync_OwnerEmailAlreadyExists_ReturnsFailure()
        {
            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByEmailAsync(
                    "pizza@example.com"))
                .ReturnsAsync((Restaurant)null);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(
                    "rahul@example.com"))
                .ReturnsAsync(new User
                {
                    Id = 10,
                    Email = "rahul@example.com"
                });

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
                OwnerName = "Rahul Sharma",
                OwnerEmail = "rahul@example.com",
                OwnerPassword = "Rahul@12345"
            };

            var result =
                await _superAdminService
                    .AddRestaurantInternalAsync(request);

            Assert.IsFalse(result.Success);

            Assert.AreEqual(
                "User with this email already exists.",
                result.Message);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurant(
                    It.IsAny<Restaurant>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddRestaurantInternalAsync_Success_CreatesRestaurantOwnerAndAddress()
        {
            _restaurantRepositoryMock
                .Setup(x => x.GetRestaurantByEmailAsync(
                    "pizza@example.com"))
                .ReturnsAsync((Restaurant)null);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(
                    "rahul@example.com"))
                .ReturnsAsync((User)null);

            _restaurantRepositoryMock
                .Setup(x => x.AddRestaurant(
                    It.IsAny<Restaurant>()))
                .Callback<Restaurant>(restaurant =>
                {
                    restaurant.Id = 1;
                    restaurant.AddressId = 20;
                });

            _userRepositoryMock
                .Setup(x => x.AddUser(
                    It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    user.Id = 10;
                });

            _restaurantRepositoryMock
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _userRepositoryMock
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
                OwnerName = "Rahul Sharma",
                OwnerEmail = "rahul@example.com",
                OwnerPassword = "Rahul@12345"
            };

            var result =
                await _superAdminService
                    .AddRestaurantInternalAsync(request);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Restaurant and owner onboarded successfully.",
                result.Message);

            _restaurantRepositoryMock.Verify(
                x => x.AddRestaurant(
                    It.Is<Restaurant>(r =>
                        r.Name == "Pizza Palace" &&
                        r.Email == "pizza@example.com" &&
                        r.IsActive)),
                Times.Once);

            _restaurantRepositoryMock.Verify(
                x => x.AddAddress(
                    It.Is<Address>(a =>
                        a.BuildingNumber == "10" &&
                        a.Locality == "Sector 18" &&
                        a.City == "Noida" &&
                        a.State == "Uttar Pradesh" &&
                        a.Country == "India" &&
                        a.PostalCode == "201301")),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUser(
                    It.Is<User>(u =>
                        u.Name == "Rahul Sharma" &&
                        u.Email == "rahul@example.com" &&
                        u.Role == UserRole.admin.ToString() &&
                        u.IsActive &&
                        u.Password != "Rahul@12345")),
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
