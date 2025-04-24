using TeachTether.Application.Common.Models;

namespace TeachTether.Application.DTOs
{
    public class CreateSchoolAdminRequest
    {
        public required CreateUserDto User { get; set; }
    }
}
