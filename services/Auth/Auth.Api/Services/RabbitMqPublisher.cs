using System.Text.Json;
using Auth.Api.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Auth.Api.Services;

public class RabbitMqPublisher : IRabbitMqPublisher
{
	private readonly ConnectionFactory _connectionFactory;
	private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

	public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
	{
		var rabbitMqOptions = options.Value;
		_connectionFactory = new ConnectionFactory
		{
			HostName = rabbitMqOptions.Host,
			Port = rabbitMqOptions.Port,
			UserName = rabbitMqOptions.User,
			Password = rabbitMqOptions.Password,
			VirtualHost = string.IsNullOrWhiteSpace(rabbitMqOptions.VirtualHost) ? "/" : rabbitMqOptions.VirtualHost,
			AutomaticRecoveryEnabled = true,
			NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
		};
	}

	public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(queueName))
		{
			throw new ArgumentException("Queue name is required.", nameof(queueName));
		}

		await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
		await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

		await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, passive: false, noWait: false, cancellationToken: cancellationToken);

		var body = JsonSerializer.SerializeToUtf8Bytes(message, _serializerOptions);
		var properties = new BasicProperties { Persistent = true };

		await channel.BasicPublishAsync(
			exchange: string.Empty,
			routingKey: queueName,
			mandatory: false,
			basicProperties: properties,
			body: body,
			cancellationToken: cancellationToken);
	}
}