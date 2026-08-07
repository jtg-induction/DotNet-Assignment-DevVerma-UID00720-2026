using System.Threading.Tasks;
using System.Web.Http;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using System.Net;

namespace Assignment3.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IHttpActionResult> SignUp(SignUpRequestDto request)
        {

            var response = await _userService.SignUpAsync(request);

            if (!response.Success)
            {
                return Content(HttpStatusCode.Conflict, response);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _userService.LoginAsync(request);

            if (response == null)
            {
                return Content(
                    HttpStatusCode.Unauthorized,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid email or password.",
                        Data = null
                    });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Login successful.",
                Data = response
            });
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IHttpActionResult> Logout(LogoutRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool isLoggedOut = await _userService.LogoutAsync(request);

            if (!isLoggedOut)
            {
                return Content(
                    HttpStatusCode.Unauthorized,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid refresh token.",
                        Data = null
                    });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logged out successfully.",
                Data = null
            });
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IHttpActionResult> RefreshToken(RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _userService.RefreshTokenAsync(request);

            if (response == null)
            {
                return Content(
                    HttpStatusCode.Unauthorized,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token.",
                        Data = null
                    });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Data = response
            });
        }
    }
}
