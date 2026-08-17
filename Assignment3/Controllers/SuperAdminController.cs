using Assignment3.DTOs;
using Assignment3.Services.Interfaces;
using System.Threading.Tasks;
using System.Web.Http;

namespace Assignment3.Controllers
{
    [Authorize(Roles = "super_admin")]
    [RoutePrefix("api/super-admin")]
    public class SuperAdminController : ApiController
    {
        private readonly ISuperAdminService _superAdminService;

        public SuperAdminController(
            ISuperAdminService superAdminService)
        {
            _superAdminService = superAdminService;
        }

        [HttpPost]
        [Route("add-restaurant")]
        public async Task<IHttpActionResult> AddRestaurant(AddRestaurantRequestDto request)
        {
            var response = await _superAdminService
                    .AddRestaurantAsync(request);

            if (!response.Success)
            {
                return Content(
                    System.Net.HttpStatusCode.BadRequest,
                    response);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("add-restaurant-owner")]
        public async Task<IHttpActionResult> AddRestaurantOwner(AddRestaurantOwnerRequestDto request)
        {
            var response = await _superAdminService
                    .AddRestaurantOwnerAsync(request);

            if (!response.Success)
            {
                return Content(
                    System.Net.HttpStatusCode.BadRequest,
                    response);
            }

            return Ok(response);
        }
    }
}
