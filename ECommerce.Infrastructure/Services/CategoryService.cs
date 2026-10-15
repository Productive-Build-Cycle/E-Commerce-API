using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    public CategoryService(ICategoryRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<CategoryResponse>> ListAdminAsync(CancellationToken ct = default) =>
        (await _repository.ListAdminAsync(ct)).Select(c => Map(c)!).ToList();

    public async Task<IReadOnlyList<CategoryResponse>> ListActiveAsync(CancellationToken ct = default) =>
        (await _repository.ListActiveAsync(ct)).Select(c => Map(c)!).ToList();

    public async Task<CategoryResponse?> GetAdminByIdAsync(int id, CancellationToken ct = default) =>
        Map(await _repository.GetByIdAsync(id, ct));

    public async Task<CategoryResponse?> GetCustomerByIdAsync(int id, CancellationToken ct = default) =>
        Map(await _repository.GetActiveByIdAsync(id, ct));

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var name = NormalizeName(request.Name);
        if (await _repository.NameExistsAsync(name, cancellationToken: ct))
            throw new ArgumentException("A category with this name already exists.");

        var category = new Category { Name = name, IsActive = false };
        await _repository.AddAsync(category, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(category)!;
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var name = NormalizeName(request.Name);
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null) return null;

        if (await _repository.NameExistsAsync(name, id, ct))
            throw new ArgumentException("A category with this name already exists.");

        category.Name = name;
        await _repository.SaveChangesAsync(ct);
        return Map(category);
    }

    public async Task<CategoryResponse?> SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null) return null;

        category.IsActive = active;
        await _repository.SaveChangesAsync(ct);
        return Map(category);
    }

    private static string NormalizeName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) throw new ArgumentException("Category name is required.");
        if (trimmed.Length > 150) throw new ArgumentException("Category name cannot exceed 150 characters.");
        return trimmed;
    }

    private static CategoryResponse? Map(Category? category) =>
        category is null ? null : new CategoryResponse(category.Id, category.Name, category.IsActive);
}
