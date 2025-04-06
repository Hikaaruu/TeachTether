using Microsoft.AspNetCore.Mvc;

namespace TeachTether.API.Errors
{
    public class ApiErrorResponse : ProblemDetails
    {
        public string? TraceId { get; set; }
    }
}
