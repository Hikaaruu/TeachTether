namespace TeachTether.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}
