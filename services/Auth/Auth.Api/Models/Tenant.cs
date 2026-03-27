namespace Auth.Api.Models
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public Subscription Subscription { get; set; }
        public DateTime SubscriptionExpiry { get; set; }
        public SubscriptionStatus SubscriptionStatus { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum Subscription
    {
        Free,
        Basic,
        Premium
    }

    public enum SubscriptionStatus
    {
        Active,
        Trialing,
        Canceled
    }

    public record RegisterTenantRequest(
        string StoreName,
        string Address,
        string PhoneNumber,
        string Email,
        string Password,
        string AdminFullName,
        Subscription Subscription = Subscription.Free
    );

    public record TenantResponse(
        Guid Id,
        string StoreName,
        string Address,
        string PhoneNumber,
        Subscription Subscription,
        DateTime SubscriptionExpiry,
        SubscriptionStatus SubscriptionStatus,
        bool IsActive,
        DateTime CreatedAt
    );

    public record RegisterTenantResponse(
        Guid TenantId,
        string Message,
        TenantResponse TenantData
    );
}