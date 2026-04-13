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

	public MomoController(
		IMomoService momoService)
	{
		_momoService = momoService;
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
}
