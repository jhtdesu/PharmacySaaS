using Auth.Api.Models;
using Auth.Api.Services;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Shared.Contracts.Models;

namespace Auth.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class MomoController : ControllerBase
{
	private readonly IMomoService _momoService;
	private readonly IMessageQueueService _messageQueueService;
	private readonly ILogger<MomoController> _logger;

	public MomoController(
		IMomoService momoService,
		IMessageQueueService messageQueueService,
		ILogger<MomoController> logger)
	{
		_momoService = momoService;
		_messageQueueService = messageQueueService;
		_logger = logger;
	}

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<BaseResponse<MomoCreatePaymentResponseModel>>> CreatePayment(
		[FromBody] OrderInfoModel order,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("MoMo payment creation requested for order {OrderId} and amount {Amount}.", order.OrderId, order.Amount);

		var response = await _momoService.CreatePaymentAsync(order, cancellationToken);
		if (!response.Success)
		{
			_logger.LogWarning("MoMo payment creation failed for order {OrderId}: {Message}", order.OrderId, response.Message);
			return BadRequest(response);
		}

		_logger.LogInformation("MoMo payment creation succeeded for order {OrderId}.", order.OrderId);
		return Ok(response);
	}

	[HttpPost("webhook")]
	[AllowAnonymous]
	public async Task<IActionResult> ReceiveWebhook(CancellationToken cancellationToken)
	{
		var webhook = await ReadWebhookAsync();
		return await HandleWebhookAsync(webhook, cancellationToken);
	}

	[HttpGet("webhook")]
	[AllowAnonymous]
	public async Task<IActionResult> ReceiveWebhookFromQuery(CancellationToken cancellationToken)
	{
		var webhook = MapFromValues(Request.Query);
		return await HandleWebhookAsync(webhook, cancellationToken);
	}

	private async Task<IActionResult> HandleWebhookAsync(MomoWebhookModel webhook, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"MoMo webhook received. OrderId={OrderId}, RequestId={RequestId}, ResultCode={ResultCode}, ContentType={ContentType}, HasForm={HasForm}",
			webhook.OrderId,
			webhook.RequestId,
			webhook.ResultCode,
			Request.ContentType,
			Request.HasFormContentType);

		if (!_momoService.IsValidWebhookSignature(webhook))
		{
			_logger.LogWarning("MoMo webhook signature validation failed for OrderId={OrderId}, RequestId={RequestId}.", webhook.OrderId, webhook.RequestId);
			return BadRequest(new BaseResponse<object>("Invalid MoMo signature."));
		}

		_logger.LogInformation("MoMo webhook signature validated. Publishing to queue for OrderId={OrderId}, RequestId={RequestId}.", webhook.OrderId, webhook.RequestId);
		await _messageQueueService.PublishMomoWebhookAsync(webhook, cancellationToken);
		_logger.LogInformation("MoMo webhook published to queue for OrderId={OrderId}, RequestId={RequestId}.", webhook.OrderId, webhook.RequestId);
		return Ok(new BaseResponse<object>(null!, "Webhook received."));
	}

	private async Task<MomoWebhookModel> ReadWebhookAsync()
	{
		if (Request.HasFormContentType)
		{
			_logger.LogDebug("Reading MoMo webhook as form payload.");
			var form = await Request.ReadFormAsync();
			return MapFromValues(form);
		}

		using var reader = new StreamReader(Request.Body, Encoding.UTF8);
		var body = await reader.ReadToEndAsync();
		_logger.LogDebug("Reading MoMo webhook body with length {Length}.", body.Length);

		if (!string.IsNullOrWhiteSpace(body))
		{
			try
			{
				var jsonWebhook = JsonSerializer.Deserialize<MomoWebhookModel>(body, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (jsonWebhook is not null)
				{
					return jsonWebhook;
				}
			}
			catch (JsonException)
			{
			}
		}

		return MapFromValues(Request.Query);
	}

	private static MomoWebhookModel MapFromValues(IQueryCollection values)
	{
		return MapFromValues(values.AsEnumerable().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase));
	}

	private static MomoWebhookModel MapFromValues(IFormCollection values)
	{
		return MapFromValues(values.AsEnumerable().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase));
	}

	private static MomoWebhookModel MapFromValues(IReadOnlyDictionary<string, string> values)
	{
		return new MomoWebhookModel
		{
			PartnerCode = GetValue(values, "partnerCode"),
			AccessKey = GetValue(values, "accessKey"),
			RequestId = GetValue(values, "requestId"),
			OrderId = GetValue(values, "orderId"),
			Amount = TryParseLong(GetValue(values, "amount")),
			OrderInfo = GetValue(values, "orderInfo"),
			OrderType = GetValue(values, "orderType"),
			TransId = TryParseLong(GetValue(values, "transId")),
			Message = GetValue(values, "message"),
			ResultCode = TryParseInt(GetValue(values, "resultCode")),
			Signature = GetValue(values, "signature"),
			PayType = GetValue(values, "payType"),
			ResponseTime = TryParseLong(GetValue(values, "responseTime")),
			ExtraData = GetValue(values, "extraData")
		};
	}

	private static string? GetValue(IReadOnlyDictionary<string, string> values, string key)
	{
		return values.TryGetValue(key, out var value) ? value : null;
	}

	private static long? TryParseLong(string? value)
	{
		return long.TryParse(value, out var parsed) ? parsed : null;
	}

	private static int? TryParseInt(string? value)
	{
		return int.TryParse(value, out var parsed) ? parsed : null;
	}
}
