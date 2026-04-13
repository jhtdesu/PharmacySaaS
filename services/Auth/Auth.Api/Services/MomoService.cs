using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Auth.Api.Models;
using Microsoft.Extensions.Options;
using Shared.Contracts.Models;

namespace Auth.Api.Services;

public class MomoService : IMomoService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly MomoOptions _momoOptions;
	private readonly ILogger<MomoService> _logger;

	public MomoService(
		IHttpClientFactory httpClientFactory,
		IOptions<MomoOptions> momoOptions,
		ILogger<MomoService> logger)
	{
		_httpClientFactory = httpClientFactory;
		_momoOptions = momoOptions.Value;
		_logger = logger;
	}

	public async Task<BaseResponse<MomoCreatePaymentResponseModel>> CreatePaymentAsync(OrderInfoModel order, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Preparing MoMo payment request for OrderId={OrderId}, Amount={Amount}.", order.OrderId, order.Amount);

		if (!ValidateConfiguration(out var configurationError))
		{
			_logger.LogWarning("MoMo payment request blocked by configuration validation: {Error}", configurationError);
			return new BaseResponse<MomoCreatePaymentResponseModel>(configurationError);
		}

		if (string.IsNullOrWhiteSpace(order.OrderId))
		{
			_logger.LogWarning("MoMo payment request blocked because OrderId was empty.");
			return new BaseResponse<MomoCreatePaymentResponseModel>("OrderId is required.");
		}

		if (order.Amount <= 0)
		{
			_logger.LogWarning("MoMo payment request blocked because Amount was invalid: {Amount}.", order.Amount);
			return new BaseResponse<MomoCreatePaymentResponseModel>("Amount must be greater than zero.");
		}

		var requestId = Guid.NewGuid().ToString("N");
		var amount = Convert.ToInt64(decimal.Round(order.Amount, 0, MidpointRounding.AwayFromZero));
		var requestType = string.IsNullOrWhiteSpace(_momoOptions.RequestType) ? "captureWallet" : _momoOptions.RequestType!;
		var lang = string.IsNullOrWhiteSpace(_momoOptions.Lang) ? "vi" : _momoOptions.Lang!;
		var orderInfo = string.IsNullOrWhiteSpace(order.OrderInfo) ? $"Payment for order {order.OrderId}" : order.OrderInfo!;

		var extraDataJson = JsonSerializer.Serialize(new { saleId = order.OrderId, fullName = order.FullName ?? string.Empty });
		var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(extraDataJson));

		var rawSignature =
			$"accessKey={_momoOptions.AccessKey}" +
			$"&amount={amount}" +
			$"&extraData={extraData}" +
			$"&ipnUrl={_momoOptions.IpnUrl}" +
			$"&orderId={order.OrderId}" +
			$"&orderInfo={orderInfo}" +
			$"&partnerCode={_momoOptions.PartnerCode}" +
			$"&redirectUrl={_momoOptions.RedirectUrl}" +
			$"&requestId={requestId}" +
			$"&requestType={requestType}";

		var signature = GenerateHmacSha256(rawSignature, _momoOptions.SecretKey!);
		_logger.LogDebug("Generated MoMo payment signature for OrderId={OrderId}.", order.OrderId);

		var payload = new
		{
			partnerCode = _momoOptions.PartnerCode,
			accessKey = _momoOptions.AccessKey,
			requestId,
			amount,
			orderId = order.OrderId,
			orderInfo,
			redirectUrl = _momoOptions.RedirectUrl,
			ipnUrl = _momoOptions.IpnUrl,
			lang,
			requestType,
			autoCapture = _momoOptions.AutoCapture,
			extraData,
			signature
		};

		var client = _httpClientFactory.CreateClient();
		client.Timeout = TimeSpan.FromSeconds(30);

		using var response = await client.PostAsJsonAsync(_momoOptions.MomoApiUrl!, payload, cancellationToken);
		_logger.LogInformation("MoMo create-payment request sent for OrderId={OrderId}; status code {StatusCode}.", order.OrderId, (int)response.StatusCode);
		var momoResponse = await response.Content.ReadFromJsonAsync<MomoCreatePaymentResponseModel>(cancellationToken: cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var message = momoResponse?.Message ?? $"MoMo API call failed with status {(int)response.StatusCode}.";
			_logger.LogWarning("MoMo create-payment API returned non-success for OrderId={OrderId}: {Message}", order.OrderId, message);
			return new BaseResponse<MomoCreatePaymentResponseModel>(message);
		}

		if (momoResponse is null)
		{
			_logger.LogWarning("MoMo create-payment API returned an empty body for OrderId={OrderId}.", order.OrderId);
			return new BaseResponse<MomoCreatePaymentResponseModel>("MoMo API returned an empty response.");
		}

		if (momoResponse.ResultCode != 0)
		{
			_logger.LogWarning("MoMo create-payment API returned ResultCode={ResultCode} for OrderId={OrderId}: {Message}", momoResponse.ResultCode, order.OrderId, momoResponse.Message);
			return new BaseResponse<MomoCreatePaymentResponseModel>(momoResponse.Message ?? "MoMo returned an unsuccessful payment initialization result.");
		}

		_logger.LogInformation("MoMo payment link created successfully for OrderId={OrderId}.", order.OrderId);
		return new BaseResponse<MomoCreatePaymentResponseModel>(momoResponse, "MoMo payment link created successfully.");
	}

	public bool IsValidWebhookSignature(MomoWebhookModel webhook)
	{
		_logger.LogDebug("Validating MoMo webhook signature for OrderId={OrderId}, RequestId={RequestId}, ResultCode={ResultCode}.", webhook.OrderId, webhook.RequestId, webhook.ResultCode);

		if (!ValidateConfiguration(out _) || string.IsNullOrWhiteSpace(webhook.Signature))
		{
			_logger.LogWarning("MoMo webhook signature validation failed early for OrderId={OrderId}, RequestId={RequestId}. Missing config or signature.", webhook.OrderId, webhook.RequestId);
			return false;
		}

		var rawSignature =
			$"accessKey={_momoOptions.AccessKey}" +
			$"&amount={webhook.Amount ?? 0}" +
			$"&extraData={webhook.ExtraData ?? string.Empty}" +
			$"&message={webhook.Message ?? string.Empty}" +
			$"&orderId={webhook.OrderId ?? string.Empty}" +
			$"&orderInfo={webhook.OrderInfo ?? string.Empty}" +
			$"&orderType={webhook.OrderType ?? string.Empty}" +
			$"&partnerCode={webhook.PartnerCode ?? string.Empty}" +
			$"&payType={webhook.PayType ?? string.Empty}" +
			$"&requestId={webhook.RequestId ?? string.Empty}" +
			$"&responseTime={webhook.ResponseTime ?? 0}" +
			$"&resultCode={webhook.ResultCode ?? -1}" +
			$"&transId={webhook.TransId ?? 0}";

		var expectedSignature = GenerateHmacSha256(rawSignature, _momoOptions.SecretKey!);
		_logger.LogDebug("MoMo webhook signature comparison completed for OrderId={OrderId}, RequestId={RequestId}.", webhook.OrderId, webhook.RequestId);
		return string.Equals(expectedSignature, webhook.Signature, StringComparison.OrdinalIgnoreCase);
	}

	private bool ValidateConfiguration(out string error)
	{
		if (string.IsNullOrWhiteSpace(_momoOptions.PartnerCode) ||
			string.IsNullOrWhiteSpace(_momoOptions.AccessKey) ||
			string.IsNullOrWhiteSpace(_momoOptions.SecretKey) ||
			string.IsNullOrWhiteSpace(_momoOptions.MomoApiUrl) ||
			string.IsNullOrWhiteSpace(_momoOptions.RedirectUrl) ||
			string.IsNullOrWhiteSpace(_momoOptions.IpnUrl))
		{
			error = "MoMo API configuration is incomplete.";
			_logger.LogWarning("MoMo configuration validation failed. PartnerCodeSet={PartnerCodeSet}, AccessKeySet={AccessKeySet}, SecretKeySet={SecretKeySet}, MomoApiUrlSet={MomoApiUrlSet}, RedirectUrlSet={RedirectUrlSet}, IpnUrlSet={IpnUrlSet}",
				!string.IsNullOrWhiteSpace(_momoOptions.PartnerCode),
				!string.IsNullOrWhiteSpace(_momoOptions.AccessKey),
				!string.IsNullOrWhiteSpace(_momoOptions.SecretKey),
				!string.IsNullOrWhiteSpace(_momoOptions.MomoApiUrl),
				!string.IsNullOrWhiteSpace(_momoOptions.RedirectUrl),
				!string.IsNullOrWhiteSpace(_momoOptions.IpnUrl));
			return false;
		}

		error = string.Empty;
		return true;
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
