using Auth.Api.Models;
using Shared.Contracts.Models;

namespace Auth.Api.Services;

public interface IMomoService
{
    Task<BaseResponse<MomoExecuteResponseModel>> CreatePaymentUrlAsync(OrderInfoModel model);
    BaseResponse<MomoExecuteResponseModel> BuildCallbackResponse(IQueryCollection collection);
    BaseResponse<string> BuildNotificationResponse();
}