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
	private readonly IMomoWebhookService _momoWebhookService;
	private readonly ILogger<MomoController> _logger;

	public MomoController(
		IMomoService momoService,
		IMomoWebhookService momoWebhookService,
		ILogger<MomoController> logger)
	{
		_momoService = momoService;
		_momoWebhookService = momoWebhookService;
		_logger = logger;
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
	public async Task<IActionResult> Webhook(
		[FromBody] MomoIpnNotificationModel notification,
		CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogError(
				"MoMo IPN received. OrderId: {OrderId}, ResultCode: {ResultCode}, TransId: {TransId}, RequestId: {RequestId}",
				notification.OrderId,
				notification.ResultCode,
				notification.TransId,
				notification.RequestId);

			await _momoWebhookService.HandleAsync(notification, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"MoMo IPN handling failed. OrderId: {OrderId}, ResultCode: {ResultCode}",
				notification.OrderId,
				notification.ResultCode);

			return BadRequest(new BaseResponse<object>(ex.Message));
		}
	}
}
