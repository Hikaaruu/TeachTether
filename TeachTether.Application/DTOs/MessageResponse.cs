namespace TeachTether.Application.DTOs;

public class MessageResponse
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public required string SenderUserId { get; set; }
    public string? Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }

    public List<MessageAttachmentResponse> Attachments { get; set; } = [];
}