using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
