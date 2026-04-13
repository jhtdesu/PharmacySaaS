namespace Auth.Api.Models;

public class RabbitMqOptions
{
	public string Host { get; set; } = "localhost";
	public int Port { get; set; } = 5672;
	public string User { get; set; } = "guest";
	public string Password { get; set; } = "guest";
	public string VirtualHost { get; set; } = "/";
	public string QueueName { get; set; } = "momo_payments";
}