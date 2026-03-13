namespace Inventory.Domain.Entities; // Adjust to your actual namespace

public class Sale : BaseData
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty; 
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}