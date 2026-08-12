using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Assignment3.DTOs.Common;

namespace Assignment3.Filters
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {

            var safeMessage = "An unexpected error occurred. Please try again later.";

            if (context.Exception is TimeoutException)
            {
                safeMessage = "The request took too long to process. Please try again.";
            }

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = safeMessage,
                Data = null
            };

            context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, response);

            // Mark exception as handled so it doesn't propagate further
            context.Exception = null;
        }
    }
}
