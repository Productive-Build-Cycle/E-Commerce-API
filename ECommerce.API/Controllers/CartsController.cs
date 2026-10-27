using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public sealed class CartsController(ICartService service) : ControllerBase
{
    private readonly ICartService _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        !TryGetUserId(out var userId) ? Unauthorized() : Ok(await _service.GetMyCartAsync(userId, ct));

    [HttpPost("items")]
    public async Task<IActionResult> Add(AddCartItemRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return Ok(await _service.AddItemAsync(userId, request, ct)); }
        catch (CartBusinessException ex) { return BusinessError(ex); }
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> Update(int cartItemId, UpdateCartItemRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return Ok(await _service.UpdateItemQuantityAsync(userId, cartItemId, request, ct)); }
        catch (CartBusinessException ex) { return BusinessError(ex); }
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> Remove(int cartItemId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return Ok(await _service.RemoveItemAsync(userId, cartItemId, ct)); }
        catch (CartBusinessException ex) { return BusinessError(ex); }
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct) =>
        !TryGetUserId(out var userId) ? Unauthorized() : Ok(await _service.ClearAsync(userId, ct));

    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private ObjectResult BusinessError(CartBusinessException ex)
    {
        var (status, message) = ex.ErrorKind switch
        {
            CartErrorKind.NotFound => (StatusCodes.Status404NotFound, "Cart item or product was not found."),
            CartErrorKind.InvalidQuantity => (StatusCodes.Status400BadRequest, "Quantity must be greater than zero."),
            CartErrorKind.ProductInactive => (StatusCodes.Status409Conflict, "Product is not available."),
            CartErrorKind.InsufficientStock => (StatusCodes.Status409Conflict, "Requested quantity exceeds available stock."),
            CartErrorKind.Duplicate => (StatusCodes.Status409Conflict, "Product already exists in the cart."),
            _ => (StatusCodes.Status400BadRequest, "Cart request could not be completed.")
        };
        return StatusCode(status, new { error = message });
    }
}
