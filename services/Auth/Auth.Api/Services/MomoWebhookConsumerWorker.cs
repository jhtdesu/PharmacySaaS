using System.Text;
using Auth.Api.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Auth.Api.Services;

public class MomoWebhookConsumerWorker : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<MomoWebhookConsumerWorker> _logger;

    private const string MomoWebhookExchange = "momo.events";
    private const string MomoWebhookQueue = "momo.webhook.queue";
    private const string MomoWebhookRoutingKey = "momo.webhook.received";

    public MomoWebhookConsumerWorker(IConnectionFactory connectionFactory, ILogger<MomoWebhookConsumerWorker> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
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

        _logger.LogInformation("Momo webhook consumer started and listening on {Queue}", MomoWebhookQueue);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Momo webhook consumer stopping");
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var consumer = sender as AsyncEventingBasicConsumer;
        var channel = consumer?.Channel;

        if (consumer is null || channel is null)
        {
            _logger.LogWarning("Momo webhook consumer received a message without an active channel");
            return;
        }

        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());
            var webhook = JsonConvert.DeserializeObject<MomoWebhookModel>(payload);

            if (webhook is null)
            {
                _logger.LogWarning("Momo webhook message could not be deserialized. DeliveryTag: {DeliveryTag}", args.DeliveryTag);
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            _logger.LogInformation(
                "Processed Momo webhook message. OrderId: {OrderId}, TransId: {TransId}, ResultCode: {ResultCode}",
                webhook.OrderId,
                webhook.TransId,
                webhook.ResultCode);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Momo webhook message. DeliveryTag: {DeliveryTag}", args.DeliveryTag);

            if (channel.IsOpen)
            {
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }
}