using TeachTether.Application.Common.Models;

namespace TeachTether.Application.DTOs
{
    public class UpdateSchoolAdminRequest
    {
        public required UpdateUserDto User { get; set; }
    }
}
