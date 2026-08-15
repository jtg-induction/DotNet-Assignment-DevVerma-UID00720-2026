using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Assignment3.DTOs.Common;

namespace Assignment3.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            foreach (var argument in actionContext.ActionArguments)
            {
                if (argument.Value == null)
                    continue;

                var properties = argument.Value.GetType()
                    .GetProperties();

                foreach (var property in properties)
                {
                    if (property.PropertyType != typeof(string))
                        continue;

                    var value = property.GetValue(argument.Value) as string;

                    if (!string.IsNullOrEmpty(value) && value != value.Trim())
                    {
                        actionContext.ModelState.AddModelError(
                            property.Name,
                            $"{property.Name} must not contain leading or trailing spaces."
                        );
                    }
                }
            }

            if (!actionContext.ModelState.IsValid)
            {
                // Map ModelState errors to Dictionary<string, List<string>> format
                var errors = actionContext.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation Failed",
                    Data = null,
                    Errors = errors
                };

                // Return 400 Bad Request
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    response
                );
            }
        }
    }
}
