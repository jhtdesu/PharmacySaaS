using System.Text.Json;
using Auth.Api.Models;
using RabbitMQ.Client;

namespace Auth.Api.Services;

public class RabbitMqMessageQueueService : IMessageQueueService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly string _queueName = "momo_payments";

    public RabbitMqMessageQueueService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task PublishPaymentSuccessAsync(MomoPaymentMessage message)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

        var json = JsonSerializer.Serialize(message);
        var body = System.Text.Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: _queueName, body: body);
    }

    public async Task<MomoPaymentMessage?> ConsumePaymentSuccessAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var result = await channel.BasicGetAsync(queue: _queueName, autoAck: false);

            if (result == null)
                return null;

            var json = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
            var message = JsonSerializer.Deserialize<MomoPaymentMessage>(json);

            await channel.BasicAckAsync(result.DeliveryTag, false);

            return message;
        }
        catch
        {
            return null;
        }
    }
}
