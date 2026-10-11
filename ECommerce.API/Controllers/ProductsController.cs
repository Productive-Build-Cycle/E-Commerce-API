using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Roles = "Customer")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await _service.ListActiveAsync(ct: ct));
    [HttpGet("category/{categoryId:int}")] public async Task<IActionResult> ByCategory(int categoryId, CancellationToken ct) => Ok(await _service.ListActiveAsync(categoryId, ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct) => (await _service.GetCustomerByIdAsync(id, ct)) is { } p ? Ok(p) : NotFound();
}
