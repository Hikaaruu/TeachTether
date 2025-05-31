namespace TeachTether.Application.DTOs;

public class MessageAttachmentResponse
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public required string FileName { get; set; }
    public required string FileType { get; set; }
    public int FileSizeBytes { get; set; }
    public required string DownloadUrl { get; set; }
}