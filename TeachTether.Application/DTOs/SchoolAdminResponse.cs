using TeachTether.Application.Common.Models;

namespace TeachTether.Application.DTOs
{
    public class SchoolAdminResponse
    {
        public int Id { get; set; }
        public required UserDto User { get; set; }
        public int SchoolId { get; set; }
    }
}
