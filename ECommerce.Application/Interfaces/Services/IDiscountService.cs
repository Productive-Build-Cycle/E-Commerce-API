using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Interfaces.Services;

public interface IDiscountService
{
    Task<IReadOnlyList<DiscountResponse>> ListAsync(
        CancellationToken ct = default);

    Task<DiscountResponse?> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task<DiscountResponse> CreateAsync(
        CreateDiscountRequest request,
        CancellationToken ct = default);

    Task<DiscountResponse?> UpdateAsync(
        int id,
        UpdateDiscountRequest request,
        CancellationToken ct = default);

    Task<DiscountResponse?> SetActiveAsync(
        int id,
        bool active,
        CancellationToken ct = default);

    Task<DiscountCalculationResponse> CalculateAsync(
    string code,
    decimal subtotal,
    CancellationToken ct = default);
}
