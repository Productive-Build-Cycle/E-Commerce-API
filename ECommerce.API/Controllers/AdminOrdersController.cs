using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly IOrderService _service;
    public AdminOrdersController(IOrderService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await _service.ListForAdminAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        (await _service.GetForAdminAsync(id, ct)) is { } order ? Ok(order) : NotFound();

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        try
        {
            var order = await _service.ChangeStatusAsync(id, request.Status, ct);
            return order is null ? NotFound() : Ok(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
