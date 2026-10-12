using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    public ProductRepository(ApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<Product>> ListAdminAsync(CancellationToken ct = default) =>
        await _context.Products.AsNoTracking().Include(p => p.Category).OrderBy(p => p.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> ListActiveAsync(int? categoryId = null, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category)
            .Where(p => p.IsActive && p.Category.IsActive);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        return await query.OrderBy(p => p.Id).ToListAsync(ct);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetActiveByIdAsync(int id, CancellationToken ct = default) =>
        _context.Products.AsNoTracking().Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive && p.Category.IsActive, ct);

    public Task<bool> ActiveCategoryExistsAsync(int categoryId, CancellationToken ct = default) =>
        _context.Categories.AnyAsync(c => c.Id == categoryId && c.IsActive, ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) => await _context.Products.AddAsync(product, ct);
    public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
}
