using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Roles = "Customer")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await _service.ListActiveAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        (await _service.GetCustomerByIdAsync(id, ct)) is { } category ? Ok(category) : NotFound();
}
