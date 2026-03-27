using Auth.Api.Data;
using Auth.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Shared.Contracts.Models;

namespace Auth.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly AuthDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public TenantController(
        AuthDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<RegisterTenantResponse>>> RegisterTenant(
        [FromBody] RegisterTenantRequest request)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.StoreName))
            return BadRequest(new BaseResponse<RegisterTenantResponse>("Store name is required."));

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new BaseResponse<RegisterTenantResponse>("Phone number is required."));

        if (string.IsNullOrWhiteSpace(request.Address))
            return BadRequest(new BaseResponse<RegisterTenantResponse>("Address is required."));

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new BaseResponse<RegisterTenantResponse>("Email is required."));

        if (string.IsNullOrWhiteSpace(request.AdminFullName))
            return BadRequest(new BaseResponse<RegisterTenantResponse>("Admin full name is required."));

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(new BaseResponse<RegisterTenantResponse>("Email already in use."));

        // Create tenant
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            StoreName = request.StoreName,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Subscription = request.Subscription,
            SubscriptionStatus = SubscriptionStatus.Trialing,
            SubscriptionExpiry = DateTime.UtcNow.AddDays(14), // 14-day trial
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new BaseResponse<RegisterTenantResponse>($"Error creating tenant: {ex.Message}"));
        }

        // Create admin user for tenant
        var adminUser = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.AdminFullName,
            TenantId = tenantId
        };

        var createUserResult = await _userManager.CreateAsync(adminUser, request.Password);
        if (!createUserResult.Succeeded)
        {
            // Delete tenant if user creation fails
            _dbContext.Tenants.Remove(tenant);
            await _dbContext.SaveChangesAsync();

            return BadRequest(new BaseResponse<RegisterTenantResponse>(
                $"Error creating admin user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}"));
        }

        // Ensure Admin role exists
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Assign Admin role to user
        await _userManager.AddToRoleAsync(adminUser, "Admin");

        // Return response
        var tenantResponse = new TenantResponse(
            tenant.Id,
            tenant.StoreName,
            tenant.Address,
            tenant.PhoneNumber,
            tenant.Subscription,
            tenant.SubscriptionExpiry,
            tenant.SubscriptionStatus,
            tenant.IsActive,
            tenant.CreatedAt
        );

        var response = new RegisterTenantResponse(
            tenantId,
            "Tenant registered successfully!",
            tenantResponse
        );

        return CreatedAtAction(nameof(RegisterTenant), response);
    }

    [HttpGet("{tenantId}")]
    public async Task<ActionResult<BaseResponse<TenantResponse>>> GetTenant(Guid tenantId)
    {
        var tenant = await _dbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return NotFound(new BaseResponse<TenantResponse>("Tenant not found."));

        var response = new TenantResponse(
            tenant.Id,
            tenant.StoreName,
            tenant.Address,
            tenant.PhoneNumber,
            tenant.Subscription,
            tenant.SubscriptionExpiry,
            tenant.SubscriptionStatus,
            tenant.IsActive,
            tenant.CreatedAt
        );

        return Ok(new BaseResponse<TenantResponse>(response, "Tenant retrieved successfully."));
    }
}
