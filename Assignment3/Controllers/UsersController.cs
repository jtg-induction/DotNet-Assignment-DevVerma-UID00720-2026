using System.Threading.Tasks;
using System.Web.Http;
using Assignment3.DTOs;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.SignUpAsync(request);

            if (!response.Success)
            {
                return Content(HttpStatusCode.Conflict, response);
            }

            return Ok(response);
        }
    }
}
