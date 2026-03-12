namespace Inventory.Application.DTOs.Medicines;

public class LowStockMedicineDTO
{
    public Guid MedicineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int TotalCurrentStock { get; set; }
    public int LowStockLevel { get; set; }
    public string Unit { get; set; } = string.Empty;
}