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

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: false, basicProperties: properties, body: body);
    }

    public async Task ProcessNextPaymentSuccessAsync(
        Func<MomoPaymentMessage, CancellationToken, Task<bool>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var result = await channel.BasicGetAsync(queue: _queueName, autoAck: false);

            if (result == null)
                return;

            var json = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
            var message = JsonSerializer.Deserialize<MomoPaymentMessage>(json);

            if (message == null)
            {
                await channel.BasicNackAsync(result.DeliveryTag, false, true);
                return;
            }

            var processed = await handler(message, cancellationToken);

            if (processed)
            {
                await channel.BasicAckAsync(result.DeliveryTag, false);
            }
            else
            {
                await channel.BasicNackAsync(result.DeliveryTag, false, true);
            }
        }
        catch
        {
            return;
        }
    }
}
