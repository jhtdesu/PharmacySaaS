using Auth.Api.Models;
using Shared.Contracts.Models;

namespace Auth.Api.Services;

public interface ITenantService
{
    Task<BaseResponse<RegisterTenantResponse>> RegisterTenantAsync(RegisterTenantRequest request);
    Task<BaseResponse<TenantResponse>> GetTenantAsync(Guid tenantId);
    Task<BaseResponse<MomoCreatePaymentResponseModel>> BuySubscriptionAsync(SubscriptionPurchaseRequest request, CancellationToken cancellationToken = default);
}