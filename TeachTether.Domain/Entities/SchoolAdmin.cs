namespace TeachTether.Domain.Entities
{
    public class SchoolAdmin
    {
        public int Id { get; set; }
        public required string UserId { get; set; } 
        public int SchoolId { get; set; }
    }
}
