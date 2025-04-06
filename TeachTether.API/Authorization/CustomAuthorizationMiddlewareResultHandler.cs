using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using TeachTether.API.Errors;

namespace TeachTether.API.Authorization
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context,
            AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Succeeded)
            {
                await next(context);
                return;
            }

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            var response = new ApiErrorResponse
            {
                Title = authorizeResult.Forbidden ? "Forbidden" : "Unauthorized",
                Status = authorizeResult.Forbidden ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized,
                Detail = "You are not authorized to access this resource.",
                Instance = context.Request.Path,
                Type = authorizeResult.Forbidden
                    ? "https://httpstatuses.com/403"
                    : "https://httpstatuses.com/401",
                TraceId = traceId
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.Status.Value;
            await context.Response.WriteAsJsonAsync(response);
        }
    }

}
