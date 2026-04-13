using Auth.Api.Models;

namespace Auth.Api.Services;

public interface IMessageQueueService
{
    Task PublishPaymentSuccessAsync(MomoPaymentMessage message);
    Task ProcessNextPaymentSuccessAsync(
        Func<MomoPaymentMessage, CancellationToken, Task<bool>> handler,
        CancellationToken cancellationToken);
}
