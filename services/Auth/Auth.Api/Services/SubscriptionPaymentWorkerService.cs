using System.Text.Json;
using Auth.Api.Data;
using Auth.Api.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Auth.Api.Services;

public class SubscriptionPaymentWorkerService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly RabbitMqOptions _rabbitMqOptions;
	private readonly ConnectionFactory _connectionFactory;
	private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

	public SubscriptionPaymentWorkerService(
		IServiceProvider serviceProvider,
		IOptions<RabbitMqOptions> rabbitMqOptions)
	{
		_serviceProvider = serviceProvider;
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

				await channel.QueueDeclareAsync(_rabbitMqOptions.SubscriptionQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null, passive: false, noWait: false, cancellationToken: stoppingToken);

				while (!stoppingToken.IsCancellationRequested)
				{
					var result = await channel.BasicGetAsync(_rabbitMqOptions.SubscriptionQueueName, autoAck: false, cancellationToken: stoppingToken);
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
			var payload = JsonSerializer.Deserialize<SubscriptionPaymentQueueMessage>(result.Body.Span, _serializerOptions);
			if (payload is null)
			{
				await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
				return;
			}

			if (payload.ResultCode is not 0 and not 9000)
			{
				await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
				return;
			}

			if (payload.TenantId == Guid.Empty)
			{
				await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
				return;
			}

			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

			var tenant = await dbContext.Tenants.FindAsync(new object[] { payload.TenantId }, cancellationToken: cancellationToken);
			if (tenant is null)
			{
				await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
				return;
			}

			tenant.Subscription = Subscription.Premium;
			tenant.SubscriptionStatus = SubscriptionStatus.Active;

			tenant.SubscriptionExpiry = tenant.SubscriptionExpiry < DateTime.UtcNow ? DateTime.UtcNow.AddMonths(1) : tenant.SubscriptionExpiry.AddMonths(1);

			dbContext.Tenants.Update(tenant);
			await dbContext.SaveChangesAsync(cancellationToken);

			await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
		}
		catch (Exception)
		{
			await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
		}
	}
}
