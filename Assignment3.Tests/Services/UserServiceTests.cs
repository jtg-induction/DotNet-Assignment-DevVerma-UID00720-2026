using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Enums;
using Assignment3.Models;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Implementations;
using Assignment3.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Assignment3.Tests.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IJwtService> _jwtServiceMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _jwtServiceMock.Object,
                _refreshTokenRepositoryMock.Object
            );
        }


        // SIGN UP TESTS

        [TestMethod]
        public async Task SignUpAsync_EmailAlreadyExists_ReturnsFailure()
        {
            var request = new SignUpRequestDto
            {
                Name = "John",
                Email = "john@test.com",
                Password = "Password123"
            };

            var existingUser = new User
            {
                Id = 1,
                Email = request.Email
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            var result = await _userService.SignUpAsync(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Email already exists.", result.Message);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task SignUpAsync_NewUser_CreatesUserSuccessfully()
        {
            var request = new SignUpRequestDto
            {
                Name = "John",
                Email = "john@test.com",
                Password = "Password123"
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            var result = await _userService.SignUpAsync(request);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("User created successfully.", result.Message);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.Is<User>(u =>
                    u.Name == request.Name &&
                    u.Email == request.Email &&
                    u.Role == UserRole.customer.ToString() &&
                    u.IsActive == true &&
                    !string.IsNullOrEmpty(u.Password)
                )),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }



        [TestMethod]
        public async Task LoginAsync_UserDoesNotExist_ReturnsNull()
        {
            var request = new LoginRequestDto
            {
                Email = "notfound@test.com",
                Password = "Password123"
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            var result = await _userService.LoginAsync(request);

            Assert.IsNull(result);

            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }


        [TestMethod]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            var password = "CorrectPassword123";

            var user = new User
            {
                Id = 1,
                Email = "john@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.customer.ToString(),
                IsActive = true
            };

            var request = new LoginRequestDto
            {
                Email = user.Email,
                Password = "WrongPassword123"
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);

            var result = await _userService.LoginAsync(request);

            Assert.IsNull(result);

            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()),
                Times.Never);
        }


        [TestMethod]
        public async Task LoginAsync_CorrectCredentials_ReturnsTokens()
        {
            var password = "Password123";

            var user = new User
            {
                Id = 1,
                Email = "john@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.customer.ToString(),
                IsActive = true
            };

            var request = new LoginRequestDto
            {
                Email = user.Email,
                Password = password
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var result = await _userService.LoginAsync(request);

            Assert.IsNotNull(result);
            Assert.AreEqual("access-token", result.AccessToken);
            Assert.AreEqual("refresh-token", result.RefreshToken);

            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(user),
                Times.Once);

            _jwtServiceMock.Verify(
                x => x.GenerateRefreshToken(),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveRefreshTokenAsync(
                    It.Is<RefreshToken>(rt =>
                        rt.UserId == user.Id &&
                        rt.Token == "refresh-token"
                    )),
                Times.Once);
        }


        // LOGOUT TESTS

        [TestMethod]
        public async Task LogoutAsync_InvalidRefreshToken_ReturnsFalse()
        {
            string refreshToken = "invalid-token";

            _refreshTokenRepositoryMock
                .Setup(x => x.GetRefreshTokenAsync(refreshToken))
                .ReturnsAsync((RefreshToken)null);

            var result = await _userService.LogoutAsync(refreshToken);

            Assert.IsFalse(result);

            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteRefreshTokenAsync(It.IsAny<RefreshToken>()),
                Times.Never);
        }


        [TestMethod]
        public async Task LogoutAsync_ValidRefreshToken_DeletesToken()
        {
            string refreshToken = "valid-token";

            var storedRefreshToken = new RefreshToken
            {
                Id = 1,
                UserId = 1,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetRefreshTokenAsync(refreshToken))
                .ReturnsAsync(storedRefreshToken);

            var result = await _userService.LogoutAsync(refreshToken);

            Assert.IsTrue(result);

            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteRefreshTokenAsync(storedRefreshToken),
                Times.Once);
        }


        // REFRESH TOKEN TESTS

        [TestMethod]
        public async Task RefreshTokenAsync_TokenDoesNotExist_ReturnsNull()
        {
            string refreshToken = "invalid-token";

            _refreshTokenRepositoryMock
                .Setup(x => x.GetRefreshTokenAsync(refreshToken))
                .ReturnsAsync((RefreshToken)null);

            var result = await _userService.RefreshTokenAsync(refreshToken);

            Assert.IsNull(result);
        }


        [TestMethod]
        public async Task RefreshTokenAsync_ExpiredToken_DeletesTokenAndReturnsNull()
        {
            string refreshToken = "expired-token";

            var storedRefreshToken = new RefreshToken
            {
                Id = 1,
                UserId = 1,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetRefreshTokenAsync(refreshToken))
                .ReturnsAsync(storedRefreshToken);

            var result = await _userService.RefreshTokenAsync(refreshToken);

            Assert.IsNull(result);

            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteRefreshTokenAsync(storedRefreshToken),
                Times.Once);

            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }


        [TestMethod]
        public async Task RefreshTokenAsync_ValidToken_RotatesTokens()
        {
            string refreshToken = "old-refresh-token";

            var user = new User
            {
                Id = 1,
                Email = "john@test.com",
                Role = UserRole.customer.ToString(),
                IsActive = true
            };

            var storedRefreshToken = new RefreshToken
            {
                Id = 1,
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                User = user
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetRefreshTokenAsync(refreshToken))
                .ReturnsAsync(storedRefreshToken);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            var result = await _userService.RefreshTokenAsync(refreshToken);

            Assert.IsNotNull(result);
            Assert.AreEqual("new-access-token", result.AccessToken);
            Assert.AreEqual("new-refresh-token", result.RefreshToken);

            // Old token deleted
            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteRefreshTokenAsync(storedRefreshToken),
                Times.Once);

            // New token created
            _refreshTokenRepositoryMock.Verify(
                x => x.SaveRefreshTokenAsync(
                    It.Is<RefreshToken>(rt =>
                        rt.UserId == user.Id &&
                        rt.Token == "new-refresh-token"
                    )),
                Times.Once);
        }


        // UPDATE PASSWORD TESTS

        [TestMethod]
        public async Task UpdatePasswordAsync_UserDoesNotExist_ReturnsFailure()
        {
            var request = new UpdatePasswordRequestDto
            {
                Email = "notfound@test.com",
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            var result = await _userService.UpdatePasswordAsync(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("User not found.", result.Message);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdatePasswordAsync_WrongCurrentPassword_ReturnsFailure()
        {
            var currentPassword = "CorrectPassword123";

            var user = new User
            {
                Id = 1,
                Email = "john@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword(currentPassword)
            };

            var request = new UpdatePasswordRequestDto
            {
                Email = user.Email,
                CurrentPassword = "WrongPassword123",
                NewPassword = "NewPassword123"
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);

            var result = await _userService.UpdatePasswordAsync(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(
                "Current password is incorrect.",
                result.Message);

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Never);
        }


        [TestMethod]
        public async Task UpdatePasswordAsync_CorrectPassword_UpdatesPassword()
        {
            var currentPassword = "OldPassword123";
            var newPassword = "NewPassword123";

            var user = new User
            {
                Id = 1,
                Email = "john@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword(currentPassword)
            };

            var request = new UpdatePasswordRequestDto
            {
                Email = user.Email,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);

            var result = await _userService.UpdatePasswordAsync(request);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(
                "Password updated successfully.",
                result.Message);

            // Verify the NEW password works
            Assert.IsTrue(
                BCrypt.Net.BCrypt.Verify(
                    newPassword,
                    user.Password));

            // Verify the OLD password no longer works
            Assert.IsFalse(
                BCrypt.Net.BCrypt.Verify(
                    currentPassword,
                    user.Password));

            _userRepositoryMock.Verify(
                x => x.SaveAsync(),
                Times.Once);
        }
    }
}
