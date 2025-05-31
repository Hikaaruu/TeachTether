using System.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;

namespace TeachTether.API.Errors;

public static class ApiErrorResponseFactory
{
    public static ApiErrorResponse FromContext(HttpContext context, int statusCode, string? title = null,
        string? detail = null)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var instance = context.Request.Path;

        return new ApiErrorResponse(statusCode,
            title ?? ReasonPhrases.GetReasonPhrase(statusCode),
            detail ?? GetDetailForStatusCode(statusCode),
            instance,
            traceId
        );
    }

    private static string GetDetailForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "The request could not be understood or was missing required parameters.",
            401 => "Authentication is required and has failed or has not yet been provided.",
            403 => "You do not have permission to access this resource.",
            404 => "The requested resource could not be found.",
            405 => "The HTTP method is not allowed for the requested resource.",
            415 => "The media type is not supported.",
            500 => "An unexpected server error occurred.",
            _ => "An error occurred while processing your request."
        };
    }
}