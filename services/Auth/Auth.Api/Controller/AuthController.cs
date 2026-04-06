
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Models;

namespace Auth.Api.Controller;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<object>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!string.IsNullOrEmpty(result.Message) && result.Message.ToLower().Contains("already in use"))
            return BadRequest(result);
        return Ok(result);
    }


    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<object>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!string.IsNullOrEmpty(result.Message) && result.Message.ToLower().Contains("invalid email or password"))
            return Unauthorized(result);
        return Ok(result);
    }


    [HttpPost("refresh")]
    public async Task<ActionResult<BaseResponse<object>>> Refresh([FromBody] TokenModel request)
    {
        var result = await _authService.RefreshAsync(request);
        if (!string.IsNullOrEmpty(result.Message) && result.Message.ToLower().Contains("invalid or expired refresh token"))
            return Unauthorized(result);
        return Ok(result);
    }

}