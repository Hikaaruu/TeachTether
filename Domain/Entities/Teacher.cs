namespace TeachTether.Domain.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}
