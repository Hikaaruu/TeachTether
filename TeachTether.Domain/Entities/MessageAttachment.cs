namespace TeachTether.Domain.Entities
{
    public class MessageAttachment
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public required string FileName { get; set; }
        public required string FileType { get; set; }
        public int FileSizeBytes { get; set; }
        public required string FileUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
