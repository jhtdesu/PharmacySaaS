using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;

namespace Auth.Api.Services;

public class RabbitMqMessageQueueService : IMessageQueueService
{
    private readonly IConnectionFactory _connectionFactory;
    private const string MomoWebhookExchange = "momo.events";
    private const string MomoWebhookQueue = "momo.webhook.queue";
    private const string MomoWebhookRoutingKey = "momo.webhook.received";

    public RabbitMqMessageQueueService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        InitializeQueueAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeQueueAsync()
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
    }

    public async Task PublishMomoWebhookAsync(object message, CancellationToken cancellationToken = default)
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
    }
}
