using System.Net;
using System.Text.Json;
using Auth.Api.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Auth.Api.Services;

public class MomoPaymentWorkerService : BackgroundService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly RabbitMqOptions _rabbitMqOptions;
	private readonly ConnectionFactory _connectionFactory;
	private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

	public MomoPaymentWorkerService(
		IHttpClientFactory httpClientFactory,
		IOptions<RabbitMqOptions> rabbitMqOptions)
	{
		_httpClientFactory = httpClientFactory;
		_rabbitMqOptions = rabbitMqOptions.Value;
		_connectionFactory = new ConnectionFactory
		{
			HostName = _rabbitMqOptions.Host,
			Port = _rabbitMqOptions.Port,
			UserName = _rabbitMqOptions.User,
			Password = _rabbitMqOptions.Password,
			VirtualHost = string.IsNullOrWhiteSpace(_rabbitMqOptions.VirtualHost) ? "/" : _rabbitMqOptions.VirtualHost,
			AutomaticRecoveryEnabled = true,
			NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
		};
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await using var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
				await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

				await channel.QueueDeclareAsync(_rabbitMqOptions.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null, passive: false, noWait: false, cancellationToken: stoppingToken);

				while (!stoppingToken.IsCancellationRequested)
				{
					var result = await channel.BasicGetAsync(_rabbitMqOptions.QueueName, autoAck: false, cancellationToken: stoppingToken);
					if (result is null)
					{
						await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
						continue;
					}

					await HandleMessageAsync(channel, result, stoppingToken);
				}
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception)
			{
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
	}

	private async Task HandleMessageAsync(IChannel channel, BasicGetResult result, CancellationToken cancellationToken)
	{
		try
		{
		var payload = JsonSerializer.Deserialize<MomoPaymentQueueMessage>(result.Body.Span, _serializerOptions);
		if (payload is null)
		{
			await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
			return;
		}		if (payload.ResultCode is not 0 and not 9000)
		{
			await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
			return;
		}		if (!Guid.TryParse(payload.OrderId, out var saleId))
		{
			await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
			return;
		}			var client = _httpClientFactory.CreateClient("InventoryApi");
			var response = await client.PostAsJsonAsync("api/medicines/checkout/complete", new CompleteCheckoutRequest(saleId), cancellationToken);

			if (response.IsSuccessStatusCode || response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
			{
				await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
				return;
			}

			await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
		}
		catch (Exception)
		{
			await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
		}
	}
}