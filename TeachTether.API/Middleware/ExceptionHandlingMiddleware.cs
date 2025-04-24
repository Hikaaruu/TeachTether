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

                int statusCode;

                switch (ex)
                {
                    case NotFoundException:
                        statusCode = StatusCodes.Status404NotFound;
                        break;

                    case ForbiddenException:
                        statusCode = StatusCodes.Status403Forbidden;
                        break;

                    case UnauthorizedAccessException:
                        statusCode = StatusCodes.Status401Unauthorized;
                        break;

                    default:
                        statusCode = StatusCodes.Status500InternalServerError;
                        break;
                }

                context.Response.StatusCode = statusCode;
                var error = ApiErrorResponseFactory.FromContext(context, statusCode,detail:ex.Message);

                await context.Response.WriteAsJsonAsync(error);
            }
        }
    }

}
