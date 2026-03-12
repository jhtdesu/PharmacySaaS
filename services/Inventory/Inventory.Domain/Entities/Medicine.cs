namespace Inventory.Domain.Entities;

public class Medicine : BaseData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty; 
    public string ActiveIngredient { get; set; } = string.Empty; 
    public string Unit { get; set; } = string.Empty; 
    public int LowStockLevel { get; set; }

    public List<MedicineBatch> Batches { get; private set; } = new();
    public int GetTotalStock() => Batches.Sum(b => b.CurrentQuantity);
}