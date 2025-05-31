namespace TeachTether.Domain.Entities;

public class Subject
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SchoolId { get; set; }
}