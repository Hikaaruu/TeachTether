namespace TeachTether.Domain.Entities;

public class MessageThread
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int GuardianId { get; set; }
    public DateTime CreatedAt { get; set; }
}