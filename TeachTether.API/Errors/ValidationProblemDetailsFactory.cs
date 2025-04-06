using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace TeachTether.API.Errors
{
    public static class ValidationProblemDetailsFactory
    {
        public static IActionResult Create(HttpContext context, ModelStateDictionary modelState)
        {
            var errors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            var response = new ApiErrorResponse
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Type = "https://httpstatuses.com/400",
                Instance = context.Request.Path,
                TraceId = traceId
            };

            response.Extensions["errors"] = errors;

            return new BadRequestObjectResult(response);
        }
    }


}
