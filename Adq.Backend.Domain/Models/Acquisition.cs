namespace Adq.Backend.Domain.Models
{
    public class Acquisition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Budget { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitValue { get; set; }
        public decimal TotalValue => Quantity * UnitValue;
        public DateTime AcquisitionDate { get; set; } = DateTime.UtcNow;
        public string Supplier { get; set; } = string.Empty;
        public string Documentation { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
