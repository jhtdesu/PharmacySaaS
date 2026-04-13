using Auth.Api.Models;
using Shared.Contracts.Models;

namespace Auth.Api.Services;

public interface IMomoService
{
    Task<BaseResponse<MomoCreatePaymentResponseModel>> CreatePaymentAsync(OrderInfoModel order, CancellationToken cancellationToken = default);
}