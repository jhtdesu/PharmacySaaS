namespace Auth.Api.Models;

public class SubscriptionPaymentQueueMessage
{
    public string OrderId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public long Amount { get; set; }
    public int ResultCode { get; set; }
    public long? TransId { get; set; }
    public string? RequestId { get; set; }
    public string? PartnerCode { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}
