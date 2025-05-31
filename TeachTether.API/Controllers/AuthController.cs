using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (result, token) = await _authService.RegisterAsync(request);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Token = token });
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);

        if (token == null)
            return Unauthorized();

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Me()
    {
        var userInfo = await _authService.GetCurrentUserInfoAsync(User);
        if (userInfo == null)
            return Unauthorized();

        return Ok(userInfo);
    }
}