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

	public MomoController(IMomoService momoService, IMessageQueueService messageQueueService)
	{
		_momoService = momoService;
		_messageQueueService = messageQueueService;
	}

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<BaseResponse<MomoCreatePaymentResponseModel>>> CreatePayment(
		[FromBody] OrderInfoModel order,
		CancellationToken cancellationToken)
	{
		var response = await _momoService.CreatePaymentAsync(order, cancellationToken);
		if (!response.Success)
		{
			return BadRequest(response);
		}

		return Ok(response);
	}

	[HttpPost("webhook")]
	[AllowAnonymous]
	public async Task<IActionResult> ReceiveWebhook([FromBody] MomoWebhookModel webhook, CancellationToken cancellationToken)
	{
		if (!_momoService.IsValidWebhookSignature(webhook))
		{
			return BadRequest(new BaseResponse<object>("Invalid MoMo signature."));
		}

		await _messageQueueService.PublishMomoWebhookAsync(webhook, cancellationToken);
		return Ok(new BaseResponse<object>(null!, "Webhook received."));
	}
}
