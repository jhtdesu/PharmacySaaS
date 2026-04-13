using Auth.Api.Models;

namespace Auth.Api.Services;

public interface IMomoWebhookService
{
	Task HandleAsync(MomoIpnNotificationModel notification, CancellationToken cancellationToken = default);
}