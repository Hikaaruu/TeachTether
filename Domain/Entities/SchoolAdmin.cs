namespace TeachTether.Domain.Entities
{
    public class SchoolAdmin
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SchoolId { get; set; }
    }
}
