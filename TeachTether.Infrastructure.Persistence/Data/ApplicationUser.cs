using Microsoft.AspNetCore.Identity;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data
{
    public class ApplicationUser : IdentityUser
    {
        public override string? UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public Sex Sex { get; set; }
        public UserType UserType { get; set; }
    }

}
