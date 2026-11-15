using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    public OrderRepository(ApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default) =>
        await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items.OrderBy(item => item.Id))
            .OrderByDescending(order => order.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items.OrderBy(item => item.Id))
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.Id)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Orders
            .Include(order => order.Items.OrderBy(item => item.Id))
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<Order?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken = default) =>
        _context.Orders
            .AsNoTracking()
            .Include(order => order.Items.OrderBy(item => item.Id))
            .FirstOrDefaultAsync(order => order.Id == id && order.UserId == userId, cancellationToken);

    public Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(user => user.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetProductsByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return [];

        return await _context.Products
            .Include(product => product.Category)
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await _context.Orders.AddAsync(order, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
