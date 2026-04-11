using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Auth.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;

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
                if (TryResolveSaleId(webhook, out var saleId))
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

    private static bool TryResolveSaleId(MomoWebhookModel webhook, out Guid saleId)
    {
        if (TryParseSaleIdFromExtraData(webhook.ExtraData, out saleId))
        {
            return true;
        }

        return Guid.TryParse(webhook.OrderId, out saleId);
    }

    private static bool TryParseSaleIdFromExtraData(string? extraData, out Guid saleId)
    {
        saleId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(extraData))
        {
            return false;
        }

        try
        {
            var normalized = extraData.Trim();
            var remainder = normalized.Length % 4;

            if (remainder > 0)
            {
                normalized = normalized.PadRight(normalized.Length + (4 - remainder), '=');
            }

            var decodedBytes = Convert.FromBase64String(normalized);
            var decodedJson = Encoding.UTF8.GetString(decodedBytes);

            if (TryParseSaleIdFromKeyValue(decodedJson, out saleId))
            {
                return true;
            }

            var metadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedJson);
            if (metadata is null)
            {
                return false;
            }

            metadata.TryGetValue("saleId", out var saleIdText);
            if (string.IsNullOrWhiteSpace(saleIdText))
            {
                metadata.TryGetValue("SaleId", out saleIdText);
            }

            return Guid.TryParse(saleIdText, out saleId);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseSaleIdFromKeyValue(string decodedText, out Guid saleId)
    {
        saleId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(decodedText))
        {
            return false;
        }

        var parts = decodedText.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!parts[0].Equals("saleId", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Guid.TryParse(parts[1], out saleId);
    }
}