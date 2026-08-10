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
        public async Task<IHttpActionResult> GetAvailableRestaurants()
        {
            var response =
                await _restaurantService.GetAvailableRestaurantsAsync();

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
