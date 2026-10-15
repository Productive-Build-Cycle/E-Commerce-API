using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public sealed class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public AdminCategoriesController(ICategoryService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await _service.ListAdminAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        (await _service.GetAdminByIdAsync(id, ct)) is { } category ? Ok(category) : NotFound();

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        try
        {
            var category = await _service.UpdateAsync(id, request, ct);
            return category is null ? NotFound() : Ok(category);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPatch("{id:int}/activation")]
    public async Task<IActionResult> SetActivation(int id, [FromQuery] bool active, CancellationToken ct)
    {
        var category = await _service.SetActiveAsync(id, active, ct);
        return category is null ? NotFound() : Ok(category);
    }
}
