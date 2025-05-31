namespace TeachTether.Domain.Entities;

public class GuardianStudent
{
    public int Id { get; set; }
    public int GuardianId { get; set; }
    public int StudentId { get; set; }
}