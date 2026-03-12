namespace Inventory.Application.DTOs.MedicineBatches;

public class ExpiringBatchDTO
{
    public Guid BatchId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public int RemainingQuantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DaysUntilExpiry => (ExpiryDate - DateTime.UtcNow).Days;
}