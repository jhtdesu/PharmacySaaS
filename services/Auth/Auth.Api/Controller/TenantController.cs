using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Models;

namespace Auth.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<RegisterTenantResponse>>> RegisterTenant(
        [FromBody] RegisterTenantRequest request)
    {
        var response = await _tenantService.RegisterTenantAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(RegisterTenant), response);
    }

    [HttpGet("{tenantId}")]
    public async Task<ActionResult<BaseResponse<TenantResponse>>> GetTenant(Guid tenantId)
    {
        var response = await _tenantService.GetTenantAsync(tenantId);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost("buy-subscription")]
    public async Task<ActionResult<BaseResponse<MomoCreatePaymentResponseModel>>> BuySubscription(
        [FromBody] SubscriptionPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _tenantService.BuySubscriptionAsync(request, cancellationToken);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
