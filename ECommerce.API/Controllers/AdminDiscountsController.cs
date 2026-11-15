
using ECommerce.Application.DTOs.Discounts;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/discounts")]
[Authorize(Roles = "Admin")]
public sealed class AdminDiscountsController : ControllerBase
{
    private readonly IDiscountService _service;

    public AdminDiscountsController(IDiscountService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDiscountRequest request,
        CancellationToken ct)
    {
        try
        {
            var discount = await _service.CreateAsync(request, ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = discount.Id },
                discount);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        return Ok(await _service.ListAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var discount = await _service.GetByIdAsync(id, ct);

        return discount is null
            ? NotFound()
            : Ok(discount);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateDiscountRequest request,
        CancellationToken ct)
    {
        try
        {
            var discount = await _service.UpdateAsync(id, request, ct);

            return discount is null
                ? NotFound()
                : Ok(discount);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:int}/activation")]
    public async Task<IActionResult> SetActivation(
        int id,
        [FromQuery] bool active,
        CancellationToken ct)
    {
        var discount = await _service.SetActiveAsync(id, active, ct);

        return discount is null
            ? NotFound()
            : Ok(discount);
    }
}
