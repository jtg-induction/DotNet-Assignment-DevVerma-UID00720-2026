using Assignment3.Controllers;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Assignment3.Tests.Controllers
{
    [TestClass]
    public class UsersControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private UsersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();

            _controller = new UsersController(
                _userServiceMock.Object
            );
        }


        // SIGN UP

        [TestMethod]
        public async Task SignUp_Success_ReturnsOk()
        {
            var request = new SignUpRequestDto
            {
                Name = "John",
                Email = "john@test.com",
                Password = "Password123"
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "User created successfully.",
                Data = null
            };

            _userServiceMock
                .Setup(x => x.SignUpAsync(request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.SignUp(request);

            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _userServiceMock.Verify(
                x => x.SignUpAsync(request),
                Times.Once);
        }


        [TestMethod]
        public async Task SignUp_EmailAlreadyExists_ReturnsConflict()
        {
            var request = new SignUpRequestDto
            {
                Name = "John",
                Email = "john@test.com",
                Password = "Password123"
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "Email already exists.",
                Data = null
            };

            _userServiceMock
                .Setup(x => x.SignUpAsync(request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.SignUp(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var conflictResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                System.Net.HttpStatusCode.Conflict,
                conflictResult.StatusCode);

            _userServiceMock.Verify(
                x => x.SignUpAsync(request),
                Times.Once);
        }


        // LOGIN

        [TestMethod]
        public async Task Login_Success_ReturnsOk()
        {
            var request = new LoginRequestDto
            {
                Email = "john@test.com",
                Password = "Password123"
            };

            var loginResponse = new LoginResponseDto
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            _userServiceMock
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync(loginResponse);

            var result = await _controller.Login(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _userServiceMock.Verify(
                x => x.LoginAsync(request),
                Times.Once);
        }


        [TestMethod]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var request = new LoginRequestDto
            {
                Email = "john@test.com",
                Password = "WrongPassword"
            };

            _userServiceMock
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync((LoginResponseDto)null);

            var result = await _controller.Login(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var unauthorizedResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                System.Net.HttpStatusCode.Unauthorized,
                unauthorizedResult.StatusCode);

            _userServiceMock.Verify(
                x => x.LoginAsync(request),
                Times.Once);
        }


        // LOGOUT

        [TestMethod]
        public async Task Logout_Success_ReturnsOk()
        {
            var request = new LogoutRequestDto
            {
                RefreshToken = "valid-refresh-token"
            };

            _userServiceMock
                .Setup(x => x.LogoutAsync(request))
                .ReturnsAsync(true);

            var result = await _controller.Logout(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _userServiceMock.Verify(
                x => x.LogoutAsync(request),
                Times.Once);
        }


        [TestMethod]
        public async Task Logout_InvalidToken_ReturnsUnauthorized()
        {
            var request = new LogoutRequestDto
            {
                RefreshToken = "invalid-token"
            };

            _userServiceMock
                .Setup(x => x.LogoutAsync(request))
                .ReturnsAsync(false);

            var result = await _controller.Logout(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var unauthorizedResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                System.Net.HttpStatusCode.Unauthorized,
                unauthorizedResult.StatusCode);

            _userServiceMock.Verify(
                x => x.LogoutAsync(request),
                Times.Once);
        }


        // REFRESH TOKEN

        [TestMethod]
        public async Task RefreshToken_Success_ReturnsOk()
        {
            var request = new RefreshTokenRequestDto
            {
                RefreshToken = "old-refresh-token"
            };

            var refreshResponse = new RefreshTokenResponseDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            _userServiceMock
                .Setup(x => x.RefreshTokenAsync(request))
                .ReturnsAsync(refreshResponse);

            var result = await _controller.RefreshToken(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _userServiceMock.Verify(
                x => x.RefreshTokenAsync(request),
                Times.Once);
        }


        [TestMethod]
        public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
        {
            var request = new RefreshTokenRequestDto
            {
                RefreshToken = "invalid-token"
            };

            _userServiceMock
                .Setup(x => x.RefreshTokenAsync(request))
                .ReturnsAsync((RefreshTokenResponseDto)null);

            var result = await _controller.RefreshToken(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var unauthorizedResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                System.Net.HttpStatusCode.Unauthorized,
                unauthorizedResult.StatusCode);

            _userServiceMock.Verify(
                x => x.RefreshTokenAsync(request),
                Times.Once);
        }


        // DEACTIVATE

        [TestMethod]
        public async Task Deactivate_Success_ReturnsOk()
        {
            var request = new DeactivateRequestDto
            {
                AccessToken = "valid-access-token"
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Account deactivated successfully.",
                Data = null
            };

            _userServiceMock
                .Setup(x => x.DeactivateUserAsync(request.AccessToken))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.Deactivate(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _userServiceMock.Verify(
                x => x.DeactivateUserAsync(request.AccessToken),
                Times.Once);
        }


        [TestMethod]
        public async Task Deactivate_EmptyAccessToken_ReturnsUnauthorized()
        {
            var request = new DeactivateRequestDto
            {
                AccessToken = ""
            };

            var result = await _controller.Deactivate(request);

            Assert.IsInstanceOfType(
                result,
                typeof(UnauthorizedResult));

            _userServiceMock.Verify(
                x => x.DeactivateUserAsync(It.IsAny<string>()),
                Times.Never);
        }


        [TestMethod]
        public async Task Deactivate_ServiceFails_ReturnsBadRequest()
        {
            var request = new DeactivateRequestDto
            {
                AccessToken = "invalid-access-token"
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "User not found.",
                Data = null
            };

            _userServiceMock
                .Setup(x => x.DeactivateUserAsync(request.AccessToken))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.Deactivate(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                System.Net.HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            _userServiceMock.Verify(
                x => x.DeactivateUserAsync(request.AccessToken),
                Times.Once);
        }


        // UPDATE PASSWORD

        [TestMethod]
        public async Task UpdatePassword_Success_ReturnsOk()
        {
            var request = new UpdatePasswordRequestDto
            {
                Email = "john@test.com",
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Password updated successfully.",
                Data = null
            };

            _userServiceMock
                .Setup(x => x.UpdatePasswordAsync(request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.UpdatePassword(request);

            Assert.IsInstanceOfType(
                result,
                typeof(OkNegotiatedContentResult<ApiResponse<object>>));

            _userServiceMock.Verify(
                x => x.UpdatePasswordAsync(request),
                Times.Once);
        }


        [TestMethod]
        public async Task UpdatePassword_ServiceFails_ReturnsBadRequest()
        {
            var request = new UpdatePasswordRequestDto
            {
                Email = "john@test.com",
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword123"
            };

            var serviceResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "Current password is incorrect.",
                Data = null
            };

            _userServiceMock
                .Setup(x => x.UpdatePasswordAsync(request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.UpdatePassword(request);

            Assert.IsInstanceOfType(
                result,
                typeof(NegotiatedContentResult<ApiResponse<object>>));

            var badRequestResult =
                (NegotiatedContentResult<ApiResponse<object>>)result;

            Assert.AreEqual(
                System.Net.HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

            _userServiceMock.Verify(
                x => x.UpdatePasswordAsync(request),
                Times.Once);
        }
    }
}
