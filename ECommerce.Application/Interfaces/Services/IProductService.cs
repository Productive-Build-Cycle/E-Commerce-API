using ECommerce.Application.DTOs.Products;

namespace ECommerce.Application.Interfaces.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> ListAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductResponse>> ListActiveAsync(int? categoryId = null, CancellationToken ct = default);
    Task<ProductResponse?> GetAdminByIdAsync(int id, CancellationToken ct = default);
    Task<ProductResponse?> GetCustomerByIdAsync(int id, CancellationToken ct = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ProductResponse?> SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task<ProductResponse?> UpdateStockAsync(int id, int stockQuantity, CancellationToken ct = default);
}
