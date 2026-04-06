using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;

namespace Auth.Api.Services;

public class RabbitMqMessageQueueService : IMessageQueueService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqMessageQueueService> _logger;
    private const string MomoWebhookExchange = "momo.events";
    private const string MomoWebhookQueue = "momo.webhook.queue";
    private const string MomoWebhookRoutingKey = "momo.webhook.received";

    public RabbitMqMessageQueueService(IConnectionFactory connectionFactory, ILogger<RabbitMqMessageQueueService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        InitializeQueueAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeQueueAsync()
    {
        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            await channel.ExchangeDeclareAsync(
                exchange: MomoWebhookExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            await channel.QueueDeclareAsync(
                queue: MomoWebhookQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.QueueBindAsync(
                queue: MomoWebhookQueue,
                exchange: MomoWebhookExchange,
                routingKey: MomoWebhookRoutingKey);

            _logger.LogInformation("RabbitMQ queue and exchange initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing RabbitMQ queue");
            throw;
        }
    }

    public async Task PublishMomoWebhookAsync(object message, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: MomoWebhookExchange,
                routingKey: MomoWebhookRoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Momo webhook message published to queue: {RoutingKey}", MomoWebhookRoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing Momo webhook message to queue");
            throw;
        }
    }
}
