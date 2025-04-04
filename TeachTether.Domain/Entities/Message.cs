namespace TeachTether.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public required string SenderUserId { get; set; }
        public string? Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
