namespace Inventory.Domain.Entities;

public abstract class BaseData
{
    public bool IsDeleted { get; set; } 
    public Guid TenantId { get; set; }
}