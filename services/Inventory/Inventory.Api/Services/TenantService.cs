using System.Security.Claims;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Api.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentTenantId()
    {
        // 1. Get the current HTTP context and the logged-in user
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            // Optional: Handle scenarios where there is no user (e.g., background jobs)
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // 2. Look for the "TenantId" claim inside their token
        var tenantClaim = user.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantClaim) || !Guid.TryParse(tenantClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Tenant ID is missing from the token.");
        }

        return tenantId;
    }
}
