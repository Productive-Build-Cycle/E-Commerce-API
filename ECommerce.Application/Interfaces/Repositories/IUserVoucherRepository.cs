using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IUserVoucherRepository
{
    Task<UserVoucher?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserVoucher?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        UserVoucher voucher,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
