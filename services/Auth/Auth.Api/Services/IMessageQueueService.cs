namespace Auth.Api.Services;

public interface IMessageQueueService
{
    Task PublishMomoWebhookAsync(object message, CancellationToken cancellationToken = default);
}
