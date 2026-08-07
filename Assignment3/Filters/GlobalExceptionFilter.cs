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
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = context.Exception.Message,
                Data = null
            };

            context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, response);

            // Mark exception as handled so it doesn't propagate further
            context.Exception = null;
        }
    }
}
