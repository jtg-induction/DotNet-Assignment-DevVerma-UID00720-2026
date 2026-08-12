using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Services.Interfaces;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Assignment3.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize]
        [Route("")]
        public async Task<IHttpActionResult> CreateOrder(
            CreateOrderRequestDto request)
        {
            var userIdClaim = ((ClaimsPrincipal)User)
                .FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            long userId = long.Parse(userIdClaim.Value);

            var response = await _orderService.CreateOrderAsync(userId,request);

            if (!response.Success)
            {
                return Content(HttpStatusCode.BadRequest,response);
            }

            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        [Route("{orderId}")]
        public async Task<IHttpActionResult> GetOrderDetails(long orderId)
        {
            var userIdClaim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            long userId = long.Parse(userIdClaim.Value);

            var response =
                await _orderService.GetOrderDetailsAsync(orderId, userId);

            if (!response.Success)
            {
                return Content(HttpStatusCode.NotFound, response);
            }

            return Ok(response);
        }
    }
}
