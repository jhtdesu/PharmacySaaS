namespace Inventory.Application.Common.Interfaces;

public interface ITenantService
{
    Guid GetCurrentTenantId();
}
