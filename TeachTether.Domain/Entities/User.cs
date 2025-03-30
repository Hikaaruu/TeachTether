namespace TeachTether.Domain.Entities
{
    public class User
    {
        public string? Id { get; set; }
        public required string UserName { get; set; }
        public string? Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
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
