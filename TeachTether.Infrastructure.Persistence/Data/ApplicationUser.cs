using Microsoft.AspNetCore.Identity;

namespace TeachTether.Infrastructure.Persistence.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } 
        public Sex Sex { get; set; } 
        public UserType UserType { get; set; } 
    }

    public enum UserType
    {
        SchoolOwner,
        SchoolAdmin,
        Teacher,
        Student,
        Guardian
    }

    public enum Sex
    {
        M,
        F
    }
}
