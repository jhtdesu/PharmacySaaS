namespace Inventory.Domain.Entities;

public class InventoryTransaction : BaseData
{
    public Guid Id { get; set; }
    public Guid MedicineBatchId { get; set; }
    public MedicineBatch MedicineBatch { get; set; } = null!;
    public Guid MedicineId { get; set; } 
    public TransactionType Type { get; set; }
    public int QuantityChange { get; set; } 
    public DateTime TransactionDate { get; set; }
    public string ReferenceNote { get; set; } = string.Empty; 
}

public enum TransactionType
{
    Receive = 1,
    Dispense = 2,
    Adjustment = 3,
    Return = 4,
    Expired = 5
}