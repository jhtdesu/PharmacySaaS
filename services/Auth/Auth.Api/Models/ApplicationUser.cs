using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Models;

public class ApplicationUser : IdentityUser
{
    
    public string FullName { get; set; } = string.Empty;
    public Guid TenantId { get; set; } 
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}