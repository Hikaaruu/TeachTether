namespace TeachTether.Application.DTOs;

public class AnnouncementResponse
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}