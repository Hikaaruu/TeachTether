using System.Diagnostics;
using TeachTether.API.Errors;
using TeachTether.Application.Common.Exceptions;

namespace TeachTether.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                context.Response.ContentType = "application/json";

                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                var error = new ApiErrorResponse
                {
                    TraceId = traceId,
                    Instance = context.Request.Path,
                    Detail = ex.Message
                };

                switch (ex)
                {
                    case NotFoundException:
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        error.Status = 404;
                        error.Title = "Resource Not Found";
                        error.Type = "https://httpstatuses.com/404";
                        break;

                    case ForbiddenException:
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        error.Status = 403;
                        error.Title = "Forbidden";
                        error.Type = "https://httpstatuses.com/403";
                        break;

                    case UnauthorizedAccessException:
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        error.Status = 401;
                        error.Title = "Unauthorized";
                        error.Type = "https://httpstatuses.com/401";
                        break;

                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        error.Status = 500;
                        error.Title = "Internal Server Error";
                        error.Type = "https://httpstatuses.com/500";
                        break;
                }

                await context.Response.WriteAsJsonAsync(error);
            }
        }
    }

}
