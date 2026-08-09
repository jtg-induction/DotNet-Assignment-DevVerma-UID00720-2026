using System.Threading.Tasks;
using System.Web.Http;
using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using System.Net;
using System.Security.Claims;

namespace Assignment3.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public UsersController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
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

        [HttpPost]
        [Authorize]
        [Route("deactivate")]
        public async Task<IHttpActionResult> Deactivate()
        {
            var userIdClaim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier);

            if(userIdClaim == null)
            {
                return Unauthorized();
            }

            long userId = long.Parse(userIdClaim.Value);

            var response = await _userService.DeactivateUserAsync(userId);

            if (!response.Success)
            {
                return Content(HttpStatusCode.BadRequest, response);
            }

            return Ok(response);
        }

        [HttpPut]
        [Route("update-password")]
        public async Task<IHttpActionResult> UpdatePassword(UpdatePasswordRequestDto request)
        {
            var response = await _userService.UpdatePasswordAsync(request);

            if (!response.Success)
            {
                return Content(HttpStatusCode.BadRequest, response);
            }

            return Ok(response);
        }
    }
}
