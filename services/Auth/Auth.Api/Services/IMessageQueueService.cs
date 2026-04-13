using Auth.Api.Models;

namespace Auth.Api.Services;

public interface IMessageQueueService
{
    Task PublishPaymentSuccessAsync(MomoPaymentMessage message);
    Task<MomoPaymentMessage?> ConsumePaymentSuccessAsync(CancellationToken cancellationToken);
}
