namespace Adq.Backend.Domain.Models
{
    public class HistoryEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AcquisitionId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Payload { get; set; } = string.Empty; // JSON string or reason
        public string? User { get; set; }
    }
}
