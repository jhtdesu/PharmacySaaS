namespace Inventory.Application.DTOs.Medicines;

public class MedicineWithStockDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    
    public int TotalStock { get; set; }
    public DateTime? EarliestExpiryDate { get; set; } 
}