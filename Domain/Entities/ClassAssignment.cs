namespace TeachTether.Domain.Entities
{
    public class ClassAssignment
    {
        public int Id { get; set; }
        public int ClassGroupId { get; set; }
        public int TeacherId { get; set; }
        public string Subject { get; set; } = string.Empty;
    }
}
