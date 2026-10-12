using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public sealed class AdminProductsController : ControllerBase
{
    private readonly IProductService _service;
    public AdminProductsController(IProductService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        try { var created = await _service.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await _service.ListAdminAsync(ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct) => (await _service.GetAdminByIdAsync(id, ct)) is { } p ? Ok(p) : NotFound();

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request, CancellationToken ct)
    {
        try { var p = await _service.UpdateAsync(id, request, ct); return p is null ? NotFound() : Ok(p); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPatch("{id:int}/activation")]
    public async Task<IActionResult> SetActivation(int id, [FromQuery] bool active, CancellationToken ct)
    {
        try { var p = await _service.SetActiveAsync(id, active, ct); return p is null ? NotFound() : Ok(p); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, UpdateStockRequest request, CancellationToken ct)
    {
        try { var p = await _service.UpdateStockAsync(id, request.StockQuantity, ct); return p is null ? NotFound() : Ok(p); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
