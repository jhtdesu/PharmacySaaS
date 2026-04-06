using Shared.Contracts.Models;

namespace Auth.Api.Services;

public interface IAuthService
{
    Task<BaseResponse<object>> RegisterAsync(RegisterRequest request);
    Task<BaseResponse<object>> LoginAsync(LoginRequest request);
    Task<BaseResponse<object>> RefreshAsync(TokenModel request);
}
