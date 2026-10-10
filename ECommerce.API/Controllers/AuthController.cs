using System.Security.Claims;
using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    #region Fields

    private readonly IAuthService _authService;


    #endregion

    #region Constructors

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    #endregion

    #region Methods

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null) return Unauthorized("Invalid email or password.");

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId)) return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId);

        if (user is null) return NotFound();

        return Ok(user);
    }

    #endregion
}
