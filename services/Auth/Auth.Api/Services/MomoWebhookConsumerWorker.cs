using System.Text;
using Auth.Api.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Auth.Api.Services;

public class MomoWebhookConsumerWorker : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _inventoryApiBaseUrl;

    private const string MomoWebhookExchange = "momo.events";
    private const string MomoWebhookQueue = "momo.webhook.queue";
    private const string MomoWebhookRoutingKey = "momo.webhook.received";

    public MomoWebhookConsumerWorker(
        IConnectionFactory connectionFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _connectionFactory = connectionFactory;
        _httpClientFactory = httpClientFactory;
        _inventoryApiBaseUrl = configuration["InventoryApi:BaseUrl"] ?? "http://inventory-api:8080";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: MomoWebhookExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: MomoWebhookQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: MomoWebhookQueue,
            exchange: MomoWebhookExchange,
            routingKey: MomoWebhookRoutingKey,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await channel.BasicConsumeAsync(
            queue: MomoWebhookQueue,
            autoAck: false,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var consumer = sender as AsyncEventingBasicConsumer;
        var channel = consumer?.Channel;

        if (consumer is null || channel is null)
        {
            return;
        }

        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());
            var webhook = JsonConvert.DeserializeObject<MomoWebhookModel>(payload);

            if (webhook is null)
            {
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (webhook.ResultCode == 0)
            {
                if (Guid.TryParse(webhook.OrderId, out var saleId))
                {
                    await CompleteSaleAsync(saleId);
                }
            }

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception)
        {
            if (channel.IsOpen)
            {
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }

    private async Task CompleteSaleAsync(Guid saleId)
    {
        var client = _httpClientFactory.CreateClient();
        var endpoint = $"{_inventoryApiBaseUrl.TrimEnd('/')}/api/Medicines/checkout/complete";

        using var response = await client.PostAsJsonAsync(endpoint, new { SaleId = saleId });
        response.EnsureSuccessStatusCode();
    }
}