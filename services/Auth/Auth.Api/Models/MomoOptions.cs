namespace Auth.Api.Models;

public class MomoOptions
{
    public string? PartnerCode { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? RequestType { get; set; }
    public string? MomoApiUrl { get; set; }
    public string? RedirectUrl { get; set; }
    public string? IpnUrl { get; set; }
    public string? ReturnUrl { get; set; }
    public string? NotifyUrl { get; set; }
}
