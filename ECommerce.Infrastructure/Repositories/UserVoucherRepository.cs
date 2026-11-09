using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class UserVoucherRepository : IUserVoucherRepository
{
    private readonly ApplicationDbContext _context;

    public UserVoucherRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<UserVoucher?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return _context.UserVouchers
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<UserVoucher?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _context.UserVouchers
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _context.UserVouchers
            .AnyAsync(x => x.Code == code, cancellationToken);
    }

    public async Task AddAsync(
        UserVoucher voucher,
        CancellationToken cancellationToken = default)
    {
        await _context.UserVouchers.AddAsync(voucher, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
