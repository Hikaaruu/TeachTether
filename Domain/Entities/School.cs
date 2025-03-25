namespace TeachTether.Domain.Entities
{
    public class School
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OwnerUserId { get; set; }
    }
}
