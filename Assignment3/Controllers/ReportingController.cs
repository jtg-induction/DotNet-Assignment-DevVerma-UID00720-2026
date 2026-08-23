using Assignment3.DTOs;
using Assignment3.Services.Interfaces;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Assignment3.Controllers
{
    [Authorize(Roles = "admin")]
    [RoutePrefix("api/reporting")]
    public class ReportingController : ApiController
    {
        private readonly IReportingService _reportingService;

        public ReportingController(
            IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet]
        [Route("top-ordered-items")]
        public async Task<IHttpActionResult> GetTopOrderedItems(
            [FromUri] TopOrderedItemsRequestDto request)
        {
            var userIdClaim = ((ClaimsPrincipal)User)
                .FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            long adminId = long.Parse(userIdClaim.Value);

            request = request ?? new TopOrderedItemsRequestDto();

            var response =
                await _reportingService.GetTopOrderedItemsAsync(
                    adminId,
                    request);

            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            var pdfContent =
                new ByteArrayContent(response.Data);

            pdfContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            pdfContent.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("inline")
                {
                    FileName = "TopOrderedItems.pdf"
                };

            var httpResponse =
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = pdfContent
                };

            return ResponseMessage(httpResponse);
        }

        [HttpGet]
        [Route("frequently-bought-together/{restaurantId:int}")]
        public async Task<IHttpActionResult> GetFrequentlyBoughtTogether(int restaurantId)
        {
            var userIdClaim = ((ClaimsPrincipal)User)
                .FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            long adminId = long.Parse(userIdClaim.Value);

            var response = await _reportingService
                    .GetFrequentlyBoughtTogetherAsync(
                        adminId,
                        restaurantId);

            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            var pdfContent = new ByteArrayContent(response.Data);

            pdfContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            pdfContent.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("inline")
                {
                    FileName = "FrequentlyBoughtTogether.pdf"
                };

            var httpResponse =
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = pdfContent
                };

            return ResponseMessage(httpResponse);
        }
    }
}
