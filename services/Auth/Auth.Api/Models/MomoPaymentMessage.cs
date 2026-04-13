namespace Auth.Api.Models;

public class MomoPaymentMessage
{
    public string? OrderId { get; set; }
    public int ResultCode { get; set; }
    public string? Message { get; set; }
    public long Amount { get; set; }
    public long TransId { get; set; }
    public string? ExtraData { get; set; }
}
