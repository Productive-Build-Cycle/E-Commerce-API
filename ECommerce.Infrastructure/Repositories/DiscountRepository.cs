using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class DiscountRepository : IDiscountRepository
{
    private readonly ApplicationDbContext _context;

    public DiscountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Discount>> ListAsync(
        CancellationToken ct = default)
    {
        return await _context.Discounts
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .ToListAsync(ct);
    }

    public Task<Discount?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return _context.Discounts
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public Task<Discount?> GetByCodeAsync(
        string code,
        CancellationToken ct = default)
    {
        return _context.Discounts
            .FirstOrDefaultAsync(d => d.Code == code, ct);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        int? excludeDiscountId = null,
        CancellationToken ct = default)
    {
        var query = _context.Discounts
            .Where(d => d.Code == code);

        if (excludeDiscountId.HasValue)
        {
            query = query.Where(
                d => d.Id != excludeDiscountId.Value);
        }

        return query.AnyAsync(ct);
    }

    public async Task AddAsync(
        Discount discount,
        CancellationToken ct = default)
    {
        await _context.Discounts.AddAsync(discount, ct);
    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
