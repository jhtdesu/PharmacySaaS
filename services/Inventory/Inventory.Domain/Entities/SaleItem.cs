namespace Inventory.Domain.Entities;

public class SaleItem : BaseData
{
    public Guid Id { get; set; }
    
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!; 

    public Guid MedicineId { get; set; }
    
    public int Quantity { get; set; }    
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}