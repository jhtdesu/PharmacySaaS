using System.ComponentModel.DataAnnotations;

namespace Inventory.Domain.Entities;

public class MedicineBatch : BaseData
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty; 
    public DateTime ExpiryDate { get; set; } 
    public DateTime ImportDate { get; set; } 
    
    public int OriginalQuantity { get; set; } 
    [ConcurrencyCheck]
    public int CurrentQuantity { get; set; }  
    
    public decimal PurchasePrice { get; set; } 

    public Guid MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    
    // -- Expiry Validation -- 
    public bool IsExpiringSoon(int months = 3) 
    {
        return ExpiryDate <= DateTime.UtcNow.AddMonths(months);
    }
    public bool IsExpired() => ExpiryDate <= DateTime.UtcNow;
}