using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
	public async Task<IActionResult> Webhook([FromBody] MomoWebhookModel webhook)
	{
		if (!_momoService.IsValidWebhookSignature(webhook))
			return StatusCode(204);

		if (webhook.ResultCode == 0)
		{
			var message = new MomoPaymentMessage
			{
				OrderId = webhook.OrderId,
				ResultCode = webhook.ResultCode ?? -1,
				Message = webhook.Message,
				Amount = webhook.Amount ?? 0,
				TransId = webhook.TransId ?? 0,
				ExtraData = webhook.ExtraData
			};

			await _messageQueueService.PublishPaymentSuccessAsync(message);
		}

		return StatusCode(204);
	}

	[HttpGet("webhook/health")]
	[AllowAnonymous]
	public IActionResult WebhookHealth()
	{
		return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
	}
}
