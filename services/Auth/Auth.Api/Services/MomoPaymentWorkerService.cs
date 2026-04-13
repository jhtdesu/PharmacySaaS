using Auth.Api.Services;
using Microsoft.Extensions.Configuration;

namespace Auth.Api.Services;

public class MomoPaymentWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public MomoPaymentWorkerService(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var messageQueueService = scope.ServiceProvider.GetRequiredService<IMessageQueueService>();

                await messageQueueService.ProcessNextPaymentSuccessAsync(async (message, token) =>
                {
                    if (message.ResultCode != 0 || string.IsNullOrWhiteSpace(message.OrderId))
                    {
                        return true;
                    }

                    if (!Guid.TryParse(message.OrderId, out var saleId))
                    {
                        return true;
                    }

                    return await CompletePaymentAsync(saleId, token);
                }, stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task<bool> CompletePaymentAsync(Guid saleId, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["InventoryApi:BaseUrl"] ?? "http://inventory-api:8080";
            var inventoryApiUrl = $"{baseUrl}/api/medicines/checkout/complete";

            var payload = new { SaleId = saleId };
            var response = await client.PostAsJsonAsync(inventoryApiUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
