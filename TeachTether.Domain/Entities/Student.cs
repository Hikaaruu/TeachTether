namespace TeachTether.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public required string UserId { get; set; } 
        public int SchoolId { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}
