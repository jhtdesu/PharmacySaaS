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

	public MomoService(IHttpClientFactory httpClientFactory, IOptions<MomoOptions> momoOptions)
	{
		_httpClientFactory = httpClientFactory;
		_momoOptions = momoOptions.Value;
	}

	public async Task<BaseResponse<MomoCreatePaymentResponseModel>> CreatePaymentAsync(OrderInfoModel order, CancellationToken cancellationToken = default)
	{
		if (!ValidateConfiguration(out var configurationError))
		{
			return new BaseResponse<MomoCreatePaymentResponseModel>(configurationError);
		}

		if (string.IsNullOrWhiteSpace(order.OrderId))
		{
			return new BaseResponse<MomoCreatePaymentResponseModel>("OrderId is required.");
		}

		if (order.Amount <= 0)
		{
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
		var momoResponse = await response.Content.ReadFromJsonAsync<MomoCreatePaymentResponseModel>(cancellationToken: cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var message = momoResponse?.Message ?? $"MoMo API call failed with status {(int)response.StatusCode}.";
			return new BaseResponse<MomoCreatePaymentResponseModel>(message);
		}

		if (momoResponse is null)
		{
			return new BaseResponse<MomoCreatePaymentResponseModel>("MoMo API returned an empty response.");
		}

		if (momoResponse.ResultCode != 0)
		{
			return new BaseResponse<MomoCreatePaymentResponseModel>(momoResponse.Message ?? "MoMo returned an unsuccessful payment initialization result.");
		}

		return new BaseResponse<MomoCreatePaymentResponseModel>(momoResponse, "MoMo payment link created successfully.");
	}

	public bool IsValidWebhookSignature(MomoWebhookModel webhook)
	{
		if (!ValidateConfiguration(out _) || string.IsNullOrWhiteSpace(webhook.Signature))
		{
			return false;
		}

		var providedSignature = webhook.Signature.Trim();

		var rawSignatures = new List<string>
		{
			BuildWebhookRawSignature(webhook, includeAccessKey: true, includeMessage: true),
			BuildWebhookRawSignature(webhook, includeAccessKey: false, includeMessage: true),
			BuildWebhookRawSignature(webhook, includeAccessKey: true, includeMessage: false),
			BuildWebhookRawSignature(webhook, includeAccessKey: false, includeMessage: false)
		};

		foreach (var rawSignature in rawSignatures)
		{
			var expectedSignature = GenerateHmacSha256(rawSignature, _momoOptions.SecretKey!);
			if (string.Equals(expectedSignature, providedSignature, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private string BuildWebhookRawSignature(MomoWebhookModel webhook, bool includeAccessKey, bool includeMessage)
	{
		var parts = new List<string>();

		if (includeAccessKey)
		{
			parts.Add($"accessKey={_momoOptions.AccessKey}");
		}

		parts.Add($"amount={webhook.Amount ?? 0}");
		parts.Add($"extraData={webhook.ExtraData ?? string.Empty}");

		if (includeMessage)
		{
			parts.Add($"message={webhook.Message ?? string.Empty}");
		}

		parts.Add($"orderId={webhook.OrderId ?? string.Empty}");
		parts.Add($"orderInfo={webhook.OrderInfo ?? string.Empty}");
		parts.Add($"orderType={webhook.OrderType ?? string.Empty}");
		parts.Add($"partnerCode={webhook.PartnerCode ?? string.Empty}");
		parts.Add($"payType={webhook.PayType ?? string.Empty}");
		parts.Add($"requestId={webhook.RequestId ?? string.Empty}");
		parts.Add($"responseTime={webhook.ResponseTime ?? 0}");
		parts.Add($"resultCode={webhook.ResultCode ?? -1}");
		parts.Add($"transId={webhook.TransId ?? 0}");

		return string.Join("&", parts);
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
