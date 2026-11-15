using System.Security.Claims;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "Customer")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    public OrdersController(IOrderService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var created = await _service.CreateAsync(userId, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        return Ok(await _service.ListForCustomerAsync(userId, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        return (await _service.GetForCustomerAsync(userId, id, ct)) is { } order
            ? Ok(order)
            : NotFound();
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out userId) && userId > 0;
    }
}
