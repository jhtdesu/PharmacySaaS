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

	public MomoController(
		IMomoService momoService,
		IMomoWebhookService momoWebhookService)
	{
		_momoService = momoService;
		_momoWebhookService = momoWebhookService;
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
			await _momoWebhookService.HandleAsync(notification, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return BadRequest(new BaseResponse<object>(ex.Message));
		}
	}
}
