namespace Inventory.Application.Common.Interfaces;

public interface ITenantService
{
    Guid GetCurrentTenantId();
    Guid GetCurrentUserId();
    string? GetCurrentUserFullName();
}
