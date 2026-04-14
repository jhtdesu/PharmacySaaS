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
	[HttpGet("webhook")]
	[AllowAnonymous]
	public async Task<IActionResult> Webhook(
		[FromQuery] string? partnerCode,
		[FromQuery] string? orderId,
		[FromQuery] string? requestId,
		[FromQuery] long amount,
		[FromQuery] string? orderInfo,
		[FromQuery] string? orderType,
		[FromQuery] long? transId,
		[FromQuery] int resultCode,
		[FromQuery] string? message,
		[FromQuery] string? payType,
		[FromQuery] long responseTime,
		[FromQuery] string? extraData,
		[FromQuery] string? signature,
		[FromQuery] string? partnerUserId,
		[FromQuery] string? storeId,
		[FromBody] MomoIpnNotificationModel? bodyNotification,
		CancellationToken cancellationToken)
	{
		try
		{
			MomoIpnNotificationModel notification;
			
			if (bodyNotification != null)
			{
				notification = bodyNotification;
			}
			else
			{
				notification = new MomoIpnNotificationModel
				{
					PartnerCode = partnerCode,
					OrderId = orderId,
					RequestId = requestId,
					Amount = amount,
					OrderInfo = orderInfo,
					OrderType = orderType,
					TransId = transId,
					ResultCode = resultCode,
					Message = message,
					PayType = payType,
					ResponseTime = responseTime,
					ExtraData = extraData,
					Signature = signature,
					PartnerUserId = partnerUserId,
					StoreId = storeId
				};
			}
			
			await _momoWebhookService.HandleAsync(notification, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return BadRequest(new BaseResponse<object>(ex.Message));
		}
	}
}
