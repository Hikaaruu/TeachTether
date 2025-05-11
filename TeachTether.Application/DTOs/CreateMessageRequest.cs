using Microsoft.AspNetCore.Http;

namespace TeachTether.Application.DTOs
{
    public class CreateMessageRequest
    {
        public string? Content { get; set; }
        public List<IFormFile> Attachments { get; set; } = [];
    }
}
