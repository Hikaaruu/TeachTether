namespace TeachTether.Domain.Entities;

public class Announcement
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public AudienceType TargetAudience { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum AudienceType
{
    Student,
    Guardian,
    StudentAndGuardian
}