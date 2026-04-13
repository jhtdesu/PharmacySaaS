namespace Auth.Api.Models;

public class MomoPaymentQueueMessage
{
	public string OrderId { get; set; } = string.Empty;
	public long Amount { get; set; }
	public int ResultCode { get; set; }
	public long? TransId { get; set; }
	public string? RequestId { get; set; }
	public string? PartnerCode { get; set; }
	public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}