using Auth.Api.Data;
using Auth.Api.Models;
using Microsoft.AspNetCore.Identity;
using Shared.Contracts.Models;

namespace Auth.Api.Services;

public class TenantService : ITenantService
{
    private readonly AuthDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public TenantService(
        AuthDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<BaseResponse<RegisterTenantResponse>> RegisterTenantAsync(RegisterTenantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StoreName))
            return new BaseResponse<RegisterTenantResponse>("Store name is required.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return new BaseResponse<RegisterTenantResponse>("Phone number is required.");

        if (string.IsNullOrWhiteSpace(request.Address))
            return new BaseResponse<RegisterTenantResponse>("Address is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return new BaseResponse<RegisterTenantResponse>("Email is required.");

        if (string.IsNullOrWhiteSpace(request.TenantFullName))
            return new BaseResponse<RegisterTenantResponse>("Tenant full name is required.");

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return new BaseResponse<RegisterTenantResponse>("Email already in use.");

        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            StoreName = request.StoreName,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Subscription = request.Subscription,
            SubscriptionStatus = SubscriptionStatus.Trialing,
            SubscriptionExpiry = DateTime.UtcNow.AddDays(14),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();

            var tenantUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.TenantFullName,
                TenantId = tenantId
            };

            var createUserResult = await _userManager.CreateAsync(tenantUser, request.Password);
            if (!createUserResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new BaseResponse<RegisterTenantResponse>(
                    $"Error creating tenant user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
            }

            if (!await _roleManager.RoleExistsAsync("Tenant"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Tenant"));
            }

            await _userManager.AddToRoleAsync(tenantUser, "Tenant");

            await transaction.CommitAsync();

            var tenantResponse = MapTenantResponse(tenant);
            var registerResponse = new RegisterTenantResponse(tenantId, "Tenant registered successfully!", tenantResponse);

            return new BaseResponse<RegisterTenantResponse>(registerResponse, "Tenant registered successfully!");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new BaseResponse<RegisterTenantResponse>($"An error occurred: {ex.Message}");
        }
    }

    public async Task<BaseResponse<TenantResponse>> GetTenantAsync(Guid tenantId)
    {
        var tenant = await _dbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return new BaseResponse<TenantResponse>("Tenant not found.");

        return new BaseResponse<TenantResponse>(MapTenantResponse(tenant), "Tenant retrieved successfully.");
    }

    private static TenantResponse MapTenantResponse(Tenant tenant)
    {
        return new TenantResponse(
            tenant.Id,
            tenant.StoreName,
            tenant.Address,
            tenant.PhoneNumber,
            tenant.Subscription,
            tenant.SubscriptionExpiry,
            tenant.SubscriptionStatus,
            tenant.IsActive,
            tenant.CreatedAt);
    }
}