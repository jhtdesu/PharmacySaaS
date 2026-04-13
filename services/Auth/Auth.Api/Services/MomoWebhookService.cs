using System.Security.Cryptography;
using System.Text;
using Auth.Api.Models;
using Microsoft.Extensions.Options;

namespace Auth.Api.Services;

public class MomoWebhookService : IMomoWebhookService
{
	private readonly IRabbitMqPublisher _publisher;
	private readonly RabbitMqOptions _rabbitMqOptions;
	private readonly MomoOptions _momoOptions;

	public MomoWebhookService(
		IRabbitMqPublisher publisher,
		IOptions<RabbitMqOptions> rabbitMqOptions,
		IOptions<MomoOptions> momoOptions)
	{
		_publisher = publisher;
		_rabbitMqOptions = rabbitMqOptions.Value;
		_momoOptions = momoOptions.Value;
	}

	public async Task HandleAsync(MomoIpnNotificationModel notification, CancellationToken cancellationToken = default)
	{
		ValidateNotification(notification);

		if (!IsSuccessful(notification.ResultCode))
		{
			return;
		}

		var queueMessage = new MomoPaymentQueueMessage
		{
			OrderId = notification.OrderId!,
			Amount = notification.Amount,
			ResultCode = notification.ResultCode,
			TransId = notification.TransId,
			RequestId = notification.RequestId,
			PartnerCode = notification.PartnerCode,
			ReceivedAtUtc = DateTime.UtcNow
		};

		await _publisher.PublishAsync(_rabbitMqOptions.QueueName, queueMessage, cancellationToken);
	}

	private void ValidateNotification(MomoIpnNotificationModel notification)
	{
		if (notification is null)
		{
			throw new InvalidOperationException("MoMo notification payload is required.");
		}

		if (string.IsNullOrWhiteSpace(notification.PartnerCode) ||
			string.IsNullOrWhiteSpace(notification.OrderId) ||
			string.IsNullOrWhiteSpace(notification.RequestId) ||
			string.IsNullOrWhiteSpace(notification.Signature))
		{
			throw new InvalidOperationException("MoMo notification payload is incomplete.");
		}

		if (!string.Equals(notification.PartnerCode, _momoOptions.PartnerCode, StringComparison.Ordinal))
		{
			throw new InvalidOperationException("Invalid MoMo partner code.");
		}

		var expectedSignature = BuildSignature(notification);
		if (!FixedTimeEquals(expectedSignature, notification.Signature))
		{
			throw new InvalidOperationException("Invalid MoMo signature.");
		}
	}

	private string BuildSignature(MomoIpnNotificationModel notification)
	{
		var rawSignature =
			$"accessKey={_momoOptions.AccessKey}" +
			$"&amount={notification.Amount}" +
			$"&extraData={notification.ExtraData ?? string.Empty}" +
			$"&message={notification.Message ?? string.Empty}" +
			$"&orderId={notification.OrderId}" +
			$"&orderInfo={notification.OrderInfo ?? string.Empty}" +
			$"&orderType={notification.OrderType ?? string.Empty}" +
			$"&partnerCode={notification.PartnerCode}" +
			$"&payType={notification.PayType ?? string.Empty}" +
			$"&requestId={notification.RequestId}" +
			$"&responseTime={notification.ResponseTime}" +
			$"&resultCode={notification.ResultCode}" +
			$"&transId={notification.TransId?.ToString() ?? string.Empty}";

		return GenerateHmacSha256(rawSignature, _momoOptions.SecretKey!);
	}

	private static bool IsSuccessful(int resultCode)
	{
		return resultCode is 0 or 9000;
	}

	private static bool FixedTimeEquals(string left, string right)
	{
		var leftBytes = Encoding.UTF8.GetBytes(left.ToLowerInvariant());
		var rightBytes = Encoding.UTF8.GetBytes(right.ToLowerInvariant());
		return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
	}

	private static string GenerateHmacSha256(string data, string secret)
	{
		var keyBytes = Encoding.UTF8.GetBytes(secret);
		var dataBytes = Encoding.UTF8.GetBytes(data);

		using var hmac = new HMACSHA256(keyBytes);
		var hashBytes = hmac.ComputeHash(dataBytes);

		return Convert.ToHexString(hashBytes).ToLowerInvariant();
	}
}