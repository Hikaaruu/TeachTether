namespace TeachTether.Application.DTOs;

public class CreateAnnouncementRequest
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string TargetAudience { get; set; }
    public List<int> ClassGroupIds { get; set; } = [];
}