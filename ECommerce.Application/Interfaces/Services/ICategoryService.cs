using ECommerce.Application.DTOs.Categories;

namespace ECommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> ListAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CategoryResponse>> ListActiveAsync(CancellationToken ct = default);
    Task<CategoryResponse?> GetAdminByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryResponse?> GetCustomerByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryResponse?> SetActiveAsync(int id, bool active, CancellationToken ct = default);
}
