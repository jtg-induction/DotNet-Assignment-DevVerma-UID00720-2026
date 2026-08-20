using Assignment3.DTOs;
using Assignment3.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Assignment3.Controllers
{
    [Authorize(Roles = "admin")]
    [RoutePrefix("api/owner")]
    public class OwnerController : ApiController
    {
        private readonly IOwnerService _ownerService;

        public OwnerController(IOwnerService ownerService)
        {
            _ownerService = ownerService;
        }

        [HttpGet]
        [Route("orders")]
        public async Task<IHttpActionResult> GetOrders([FromUri] OwnerOrderDashboardRequestDto request)
        {
            var userIdClaim =
                ((System.Security.Claims.ClaimsPrincipal)User)
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            long userId = long.Parse(userIdClaim.Value);

            request = request ?? new OwnerOrderDashboardRequestDto();

            var response = await _ownerService.GetOwnerOrdersAsync(userId, request);

            if (!response.Success)
            {
                return Content(
                    System.Net.HttpStatusCode.BadRequest,
                    response);
            }

            return Ok(response);
        }

        [HttpPut]
        [Route("update-order-status")]
        public async Task<IHttpActionResult> UpdateOrderStatus(UpdateOrderStatusRequestDto request)
        {
            var userIdClaim = ((ClaimsPrincipal)User)
                .FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            long userId = long.Parse(userIdClaim.Value);

            var response = await _ownerService
                    .UpdateOrderStatusAsync(
                        request,
                        userId);

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
