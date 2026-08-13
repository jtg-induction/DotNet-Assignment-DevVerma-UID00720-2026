using Assignment3.DTOs;
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
    public class RestaurantServiceTests
    {
        private Mock<IRestaurantRepository> _restaurantRepositoryMock;
        private RestaurantService _restaurantService;

        [TestInitialize]
        public void Setup()
        {
            _restaurantRepositoryMock =
                new Mock<IRestaurantRepository>();

            _restaurantService =
                new RestaurantService(
                    _restaurantRepositoryMock.Object);
        }


        // GET AVAILABLE RESTAURANTS

        [TestMethod]
        public async Task GetAvailableRestaurantsAsync_ReturnsSuccess()
        {
            // Arrange

            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Name = "Pizza Palace",
                    Email = "pizza@test.com",
                    AddressId = 10
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Burger House",
                    Email = "burger@test.com",
                    AddressId = 20
                }
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetAvailableRestaurantsAsync(1, 10))
                .ReturnsAsync((restaurants, 2));

            // Act

            var result =
                await _restaurantService
                    .GetAvailableRestaurantsAsync(1, 10);

            // Assert

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Restaurants retrieved successfully.",
                result.Message);

            Assert.IsNotNull(result.Data);

            _restaurantRepositoryMock.Verify(
                x => x.GetAvailableRestaurantsAsync(1, 10),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurantsAsync_MapsRestaurantsCorrectly()
        {
            // Arrange

            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Name = "Pizza Palace",
                    Email = "pizza@test.com",
                    AddressId = 10
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Burger House",
                    Email = "burger@test.com",
                    AddressId = 20
                }
            };

            _restaurantRepositoryMock
                .Setup(x => x.GetAvailableRestaurantsAsync(1, 10))
                .ReturnsAsync((restaurants, 2));

            // Act

            var result =
                await _restaurantService
                    .GetAvailableRestaurantsAsync(1, 10);

            // Assert

            Assert.IsNotNull(result.Data);

            var dataType = result.Data.GetType();

            var restaurantsProperty =
                dataType.GetProperty("Restaurants");

            Assert.IsNotNull(restaurantsProperty);

            var restaurantDtos =
                restaurantsProperty.GetValue(result.Data)
                as List<RestaurantDto>;

            Assert.IsNotNull(restaurantDtos);

            Assert.AreEqual(2, restaurantDtos.Count);

            Assert.AreEqual(
                1,
                restaurantDtos[0].Id);

            Assert.AreEqual(
                "Pizza Palace",
                restaurantDtos[0].Name);

            Assert.AreEqual(
                "pizza@test.com",
                restaurantDtos[0].Email);

            Assert.AreEqual(
                10,
                restaurantDtos[0].AddressId);

            Assert.AreEqual(
                2,
                restaurantDtos[1].Id);

            Assert.AreEqual(
                "Burger House",
                restaurantDtos[1].Name);

            Assert.AreEqual(
                "burger@test.com",
                restaurantDtos[1].Email);

            Assert.AreEqual(
                20,
                restaurantDtos[1].AddressId);

            _restaurantRepositoryMock.Verify(
                x => x.GetAvailableRestaurantsAsync(1, 10),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurantsAsync_ReturnsCorrectPageInformation()
        {
            // Arrange

            int page = 2;
            int pageSize = 10;
            int totalCount = 25;

            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 11,
                    Name = "Restaurant 11",
                    Email = "r11@test.com",
                    AddressId = 11
                },
                new Restaurant
                {
                    Id = 12,
                    Name = "Restaurant 12",
                    Email = "r12@test.com",
                    AddressId = 12
                }
            };

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize))
                .ReturnsAsync(
                    (restaurants, totalCount));

            // Act

            var result =
                await _restaurantService
                    .GetAvailableRestaurantsAsync(
                        page,
                        pageSize);

            // Assert

            Assert.IsNotNull(result.Data);

            var dataType = result.Data.GetType();

            var pageProperty =
                dataType.GetProperty("Page");

            var pageSizeProperty =
                dataType.GetProperty("PageSize");

            var totalItemsProperty =
                dataType.GetProperty("TotalItems");

            var totalPagesProperty =
                dataType.GetProperty("TotalPages");

            Assert.IsNotNull(pageProperty);
            Assert.IsNotNull(pageSizeProperty);
            Assert.IsNotNull(totalItemsProperty);
            Assert.IsNotNull(totalPagesProperty);

            var actualPage =
                (int)pageProperty.GetValue(result.Data);

            var actualPageSize =
                (int)pageSizeProperty.GetValue(result.Data);

            var actualTotalItems =
                (int)totalItemsProperty.GetValue(result.Data);

            var actualTotalPages =
                (int)totalPagesProperty.GetValue(result.Data);

            Assert.AreEqual(
                page,
                actualPage);

            Assert.AreEqual(
                pageSize,
                actualPageSize);

            Assert.AreEqual(
                totalCount,
                actualTotalItems);

            Assert.AreEqual(
                3,
                actualTotalPages);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurantsAsync_CalculatesTotalPagesCorrectly()
        {
            // Arrange

            int page = 1;
            int pageSize = 10;
            int totalCount = 25;

            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Name = "Restaurant 1",
                    Email = "r1@test.com",
                    AddressId = 1
                }
            };

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize))
                .ReturnsAsync(
                    (restaurants, totalCount));

            // Act

            var result =
                await _restaurantService
                    .GetAvailableRestaurantsAsync(
                        page,
                        pageSize);

            // Assert

            Assert.IsNotNull(result.Data);

            var totalPagesProperty =
                result.Data
                    .GetType()
                    .GetProperty("TotalPages");

            Assert.IsNotNull(totalPagesProperty);

            var totalPages =
                (int)totalPagesProperty.GetValue(result.Data);

            Assert.AreEqual(
                3,
                totalPages);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurantsAsync_ExactPageCountReturnsCorrectTotalPages()
        {
            // Arrange

            int page = 1;
            int pageSize = 10;
            int totalCount = 20;

            var restaurants = new List<Restaurant>();

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize))
                .ReturnsAsync(
                    (restaurants, totalCount));

            // Act

            var result =
                await _restaurantService
                    .GetAvailableRestaurantsAsync(
                        page,
                        pageSize);

            // Assert

            Assert.IsNotNull(result.Data);

            var totalPagesProperty =
                result.Data
                    .GetType()
                    .GetProperty("TotalPages");

            Assert.IsNotNull(totalPagesProperty);

            var totalPages =
                (int)totalPagesProperty.GetValue(result.Data);

            Assert.AreEqual(
                2,
                totalPages);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurantsAsync_LessThanPageSizeReturnsOnePage()
        {
            // Arrange

            int page = 1;
            int pageSize = 10;
            int totalCount = 5;

            var restaurants = new List<Restaurant>();

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize))
                .ReturnsAsync(
                    (restaurants, totalCount));

            // Act

            var result =
                await _restaurantService
                    .GetAvailableRestaurantsAsync(
                        page,
                        pageSize);

            // Assert

            Assert.IsNotNull(result.Data);

            var totalPagesProperty =
                result.Data
                    .GetType()
                    .GetProperty("TotalPages");

            Assert.IsNotNull(totalPagesProperty);

            var totalPages =
                (int)totalPagesProperty.GetValue(result.Data);

            Assert.AreEqual(
                1,
                totalPages);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableRestaurantsAsync_ZeroRestaurantsReturnsZeroPages()
        {
            // Arrange

            int page = 1;
            int pageSize = 10;
            int totalCount = 0;

            var restaurants = new List<Restaurant>();

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize))
                .ReturnsAsync(
                    (restaurants, totalCount));

            // Act

            var result =
                await _restaurantService
                    .GetAvailableRestaurantsAsync(
                        page,
                        pageSize);

            // Assert

            Assert.IsNotNull(result.Data);

            var totalPagesProperty =
                result.Data
                    .GetType()
                    .GetProperty("TotalPages");

            Assert.IsNotNull(totalPagesProperty);

            var totalPages =
                (int)totalPagesProperty.GetValue(result.Data);

            Assert.AreEqual(
                0,
                totalPages);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableRestaurantsAsync(
                        page,
                        pageSize),
                Times.Once);
        }


        // GET AVAILABLE MENU ITEMS

        [TestMethod]
        public async Task GetAvailableMenuItemsAsync_ReturnsSuccess()
        {
            // Arrange

            int restaurantId = 1;

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 1,
                    Name = "Margherita Pizza",
                    Price = 250,
                    Description = "Cheese pizza",
                    Stock = 10
                }
            };

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableMenuItemsAsync(
                        restaurantId))
                .ReturnsAsync(menuItems);

            // Act

            var result =
                await _restaurantService
                    .GetAvailableMenuItemsAsync(
                        restaurantId);

            // Assert

            Assert.IsNotNull(result);

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                "Menu items retrieved successfully.",
                result.Message);

            Assert.IsNotNull(result.Data);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableMenuItemsAsync(
                        restaurantId),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableMenuItemsAsync_MapsMenuItemsCorrectly()
        {
            // Arrange

            int restaurantId = 1;

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 1,
                    Name = "Margherita Pizza",
                    Price = 250,
                    Description = "Cheese pizza",
                    Stock = 10
                },
                new MenuItem
                {
                    Id = 2,
                    RestaurantId = 1,
                    Name = "Farmhouse Pizza",
                    Price = 350,
                    Description = "Veggie pizza",
                    Stock = 5
                }
            };

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableMenuItemsAsync(
                        restaurantId))
                .ReturnsAsync(menuItems);

            // Act

            var result =
                await _restaurantService
                    .GetAvailableMenuItemsAsync(
                        restaurantId);

            // Assert

            Assert.IsNotNull(result.Data);

            var menuItemDtos =
                result.Data as List<MenuItemDto>;

            Assert.IsNotNull(menuItemDtos);

            Assert.AreEqual(
                2,
                menuItemDtos.Count);

            Assert.AreEqual(
                1,
                menuItemDtos[0].Id);

            Assert.AreEqual(
                1,
                menuItemDtos[0].RestaurantId);

            Assert.AreEqual(
                "Margherita Pizza",
                menuItemDtos[0].Name);

            Assert.AreEqual(
                250,
                menuItemDtos[0].Price);

            Assert.AreEqual(
                "Cheese pizza",
                menuItemDtos[0].Description);

            Assert.AreEqual(
                10,
                menuItemDtos[0].Stock);

            Assert.AreEqual(
                2,
                menuItemDtos[1].Id);

            Assert.AreEqual(
                1,
                menuItemDtos[1].RestaurantId);

            Assert.AreEqual(
                "Farmhouse Pizza",
                menuItemDtos[1].Name);

            Assert.AreEqual(
                350,
                menuItemDtos[1].Price);

            Assert.AreEqual(
                "Veggie pizza",
                menuItemDtos[1].Description);

            Assert.AreEqual(
                5,
                menuItemDtos[1].Stock);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableMenuItemsAsync(
                        restaurantId),
                Times.Once);
        }


        [TestMethod]
        public async Task GetAvailableMenuItemsAsync_NoMenuItemsReturnsEmptyList()
        {
            // Arrange

            int restaurantId = 1;

            _restaurantRepositoryMock
                .Setup(x =>
                    x.GetAvailableMenuItemsAsync(
                        restaurantId))
                .ReturnsAsync(
                    new List<MenuItem>());

            // Act

            var result =
                await _restaurantService
                    .GetAvailableMenuItemsAsync(
                        restaurantId);

            // Assert

            Assert.IsTrue(result.Success);

            Assert.IsNotNull(result.Data);

            var menuItemDtos =
                result.Data as List<MenuItemDto>;

            Assert.IsNotNull(menuItemDtos);

            Assert.AreEqual(
                0,
                menuItemDtos.Count);

            _restaurantRepositoryMock.Verify(
                x =>
                    x.GetAvailableMenuItemsAsync(
                        restaurantId),
                Times.Once);
        }
    }
}
