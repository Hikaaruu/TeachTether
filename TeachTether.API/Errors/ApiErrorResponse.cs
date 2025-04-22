using Microsoft.AspNetCore.Mvc;

namespace TeachTether.API.Errors
{
    public class ApiErrorResponse : ProblemDetails
    {
        public string? TraceId { get; set; }

        public ApiErrorResponse(int statusCode, string title, string detail, string instance, string traceId)
        {
            Status = statusCode;
            Title = title;
            Detail = detail;
            Type = $"https://httpstatuses.com/{statusCode}";
            Instance = instance;
            TraceId = traceId;
        }
    }
}
