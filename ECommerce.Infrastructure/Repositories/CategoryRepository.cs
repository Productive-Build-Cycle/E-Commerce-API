using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    public CategoryRepository(ApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<Category>> ListAdminAsync(CancellationToken ct = default) =>
        await _context.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> ListActiveAsync(CancellationToken ct = default) =>
        await _context.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Id).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Category?> GetActiveByIdAsync(int id, CancellationToken ct = default) =>
        _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);

    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var normalized = name.ToLower();
        return _context.Categories.AnyAsync(
            c => c.Name.ToLower() == normalized && (excludeId == null || c.Id != excludeId), ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await _context.Categories.AddAsync(category, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
