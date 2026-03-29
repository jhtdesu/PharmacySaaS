namespace Auth.Api.Models;

public class OrderInfoModel
{
    public string? OrderId { get; set; }
    public string? FullName { get; set; }
    public decimal Amount { get; set; }
    public string? OrderInfo { get; set; }
}
