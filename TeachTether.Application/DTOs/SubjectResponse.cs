namespace TeachTether.Application.DTOs
{
    public class SubjectResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int SchoolId { get; set; }
    }
}
