using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using System.Threading.Tasks;
using System.Web.Http;

namespace Assignment3.Controllers
{
    [RoutePrefix("api/restaurants")]
    public class RestaurantsController : ApiController
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantsController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAvailableRestaurants(int page = 1,int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 50)
            {
                return BadRequest("Page and pageSize must be greater than 0 and pageSize must be between 1 and 50");
            }

            var response = await _restaurantService.GetAvailableRestaurantsAsync(page, pageSize);

            return Ok(response);
        }

        [HttpGet]
        [Route("{restaurantId}/menu-items")]
        public async Task<IHttpActionResult> GetMenuItems(int restaurantId)
        {
            var response = await _restaurantService.GetAvailableMenuItemsAsync(restaurantId);

            return Ok(response);
        }
    }
}
