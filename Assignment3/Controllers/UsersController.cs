using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Models;
using Assignment3.Services.Interfaces;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

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

            var result = Request.CreateResponse(
                HttpStatusCode.OK,
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Login successful.",
                    Data = new
                    {
                        AccessToken = response.AccessToken
                    }
                });

            SetRefreshTokenCookie(result, response.RefreshToken);

            return ResponseMessage(result);
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IHttpActionResult> Logout()
        {

            string refreshToken = GetRefreshTokenFromCookie();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Content(
                    HttpStatusCode.Unauthorized,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Refresh token cookie not found.",
                        Data = null
                    });
            }

            bool isLoggedOut = await _userService.LogoutAsync(refreshToken);

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

            var result = Request.CreateResponse(HttpStatusCode.OK, new ApiResponse<object>
                {
                    Success = true,
                    Message = "Logged out successfully.",
                    Data = null
                });

            ClearRefreshTokenCookie(result);

            return ResponseMessage(result);
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IHttpActionResult> RefreshToken()
        {

            string refreshToken = GetRefreshTokenFromCookie();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Content(
                    HttpStatusCode.Unauthorized,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Refresh token cookie not found.",
                        Data = null
                    });
            }

            var response = await _userService.RefreshTokenAsync(refreshToken);

            if (response == null)
            {
                var failedResult = Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token.",
                        Data = null
                    });

                ClearRefreshTokenCookie(failedResult);

                return ResponseMessage(failedResult);
            }

            var result = Request.CreateResponse(
                HttpStatusCode.OK,
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    Data = new
                    {
                        AccessToken = response.AccessToken
                    }
                });

            SetRefreshTokenCookie(result, response.RefreshToken);

            return ResponseMessage(result);
        }

        private string GetRefreshTokenFromCookie()
        {
            var cookieHeader = Request.Headers.GetCookies().FirstOrDefault();

            if (cookieHeader == null)
                return null;

            var refreshTokenCookie = cookieHeader.Cookies
                .FirstOrDefault(x => x.Name == "refreshToken");

            if (refreshTokenCookie == null)
                return null;

            return refreshTokenCookie.Value;
        }

        private void SetRefreshTokenCookie(HttpResponseMessage response,string refreshToken)
        {
            var cookie = new CookieHeaderValue("refreshToken", refreshToken);

            cookie.HttpOnly = true;
            cookie.Secure = true;
            cookie.Path = "/";

            response.Headers.Add("Set-Cookie",cookie.ToString());
        }

        private void ClearRefreshTokenCookie(HttpResponseMessage response)
        {
            var cookie = new CookieHeaderValue("refreshToken", "");

            cookie.HttpOnly = true;
            cookie.Secure = true;
            cookie.Path = "/";
            cookie.Expires = System.DateTimeOffset.UtcNow.AddDays(-1);

            response.Headers.Add("Set-Cookie", cookie.ToString());
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
        [Authorize]
        [Route("update-password")]
        public async Task<IHttpActionResult> UpdatePassword(UpdatePasswordRequestDto request)
        {

            var userIdClaim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            long userId = long.Parse(userIdClaim.Value);

            var response = await _userService.UpdatePasswordAsync(request,userId);

            if (!response.Success)
            {
                return Content(HttpStatusCode.BadRequest, response);
            }

            return Ok(response);
        }
    }
}
