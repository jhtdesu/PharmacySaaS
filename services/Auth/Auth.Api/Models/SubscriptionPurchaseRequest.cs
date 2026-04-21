namespace Auth.Api.Models;

public record SubscriptionPurchaseRequest(
    Guid TenantId,
    string FullName
);
