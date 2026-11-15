using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IDiscountRepository
{
    Task<IReadOnlyList<Discount>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<Discount?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Discount?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(
        string code,
        int? excludeDiscountId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Discount discount,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
