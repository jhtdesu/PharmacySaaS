namespace Inventory.Domain.Entities;

public class Sale : BaseData
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty; 
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public SaleStatus SaleStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid ProcessedBy { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}

public enum SaleStatus
{
    Pending,
    Completed,
    Cancelled
}