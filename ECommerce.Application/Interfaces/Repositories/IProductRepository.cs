using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListAdminAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> ListActiveAsync(int? categoryId = null, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ActiveCategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
