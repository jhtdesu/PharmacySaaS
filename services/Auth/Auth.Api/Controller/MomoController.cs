using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Auth.Api.Models;
using Auth.Api.Services;

namespace Auth.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class MomoController : ControllerBase
{
    private readonly IMomoService _momoService;

    public MomoController(IMomoService momoService)
    {
        _momoService = momoService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
    {
        var response = await _momoService.CreatePaymentUrlAsync(model);
        return Ok(response);
    }

    [HttpGet("callback")]
    public IActionResult PaymentCallBack()
    {
        return Ok(_momoService.BuildCallbackResponse(HttpContext.Request.Query));
    }

    [HttpPost("notify")]
    public IActionResult PaymentNotify()
    {
        return Ok(_momoService.BuildNotificationResponse());
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] MomoWebhookModel webhook, CancellationToken cancellationToken)
    {
        var response = await _momoService.BuildWebhookResponseAsync(webhook, cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}

