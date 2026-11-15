using ECommerce.Application.DTOs.Discounts;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Services;

public sealed class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _repository;
    private readonly IUserVoucherRepository _userVoucherRepository;

    public DiscountService(
        IDiscountRepository repository,
        IUserVoucherRepository userVoucherRepository)
    {
        _repository = repository;
        _userVoucherRepository = userVoucherRepository;
    }

    public async Task<IReadOnlyList<DiscountResponse>> ListAsync(
        CancellationToken ct = default)
    {
        return (await _repository.ListAsync(ct))
            .Select(Map)
            .ToList()!;
    }

    public async Task<DiscountResponse?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return Map(await _repository.GetByIdAsync(id, ct));
    }

    public async Task<DiscountResponse> CreateAsync(
        CreateDiscountRequest request,
        CancellationToken ct = default)
    {
        Validate(request);

        var code = NormalizeCode(request.Code);

        if (await _repository.CodeExistsAsync(code, cancellationToken: ct))
            throw new ArgumentException("A discount with this code already exists.");

        var discount = new Discount
        {
            Code = code,
            Type = request.Type,
            Value = request.Value,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaximumDiscountAmount = request.MaximumDiscountAmount,
            IsActive = false
        };

        await _repository.AddAsync(discount, ct);
        await _repository.SaveChangesAsync(ct);

        return Map(discount)!;
    }

    public async Task<DiscountResponse?> UpdateAsync(
        int id,
        UpdateDiscountRequest request,
        CancellationToken ct = default)
    {
        Validate(request);

        var discount = await _repository.GetByIdAsync(id, ct);

        if (discount is null)
            return null;

        var code = NormalizeCode(request.Code);

        if (await _repository.CodeExistsAsync(code, id, ct))
            throw new ArgumentException("A discount with this code already exists.");

        discount.Code = code;
        discount.Type = request.Type;
        discount.Value = request.Value;
        discount.StartDate = request.StartDate;
        discount.EndDate = request.EndDate;
        discount.MinimumOrderAmount = request.MinimumOrderAmount;
        discount.MaximumDiscountAmount = request.MaximumDiscountAmount;

        await _repository.SaveChangesAsync(ct);

        return Map(discount);
    }

    public async Task<DiscountResponse?> SetActiveAsync(
        int id,
        bool active,
        CancellationToken ct = default)
    {
        var discount = await _repository.GetByIdAsync(id, ct);

        if (discount is null)
            return null;

        discount.IsActive = active;

        await _repository.SaveChangesAsync(ct);

        return Map(discount);
    }

    private static void Validate(CreateDiscountRequest request)
    {
        if (!Enum.IsDefined(typeof(DiscountType), request.Type))
            throw new ArgumentException("Invalid discount type.");

        if (request.Value <= 0)
            throw new ArgumentException("Discount value must be greater than zero.");

        if (request.Type == DiscountType.Percentage && request.Value > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100.");

        if (request.StartDate >= request.EndDate)
            throw new ArgumentException("Start date must be earlier than end date.");

        if (request.MinimumOrderAmount < 0)
            throw new ArgumentException("Minimum order amount cannot be negative.");

        if (request.MaximumDiscountAmount < 0)
            throw new ArgumentException("Maximum discount amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Discount code is required.");

        if (request.Code.Trim().Length > 50)
            throw new ArgumentException("Discount code cannot exceed 50 characters.");
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private static DiscountResponse? Map(Discount? discount)
    {
        return discount is null
            ? null
            : new DiscountResponse(
                discount.Id,
                discount.Code,
                discount.Type,
                discount.Value,
                discount.StartDate,
                discount.EndDate,
                discount.MinimumOrderAmount,
                discount.MaximumDiscountAmount,
                discount.IsActive);
    }

    public async Task<DiscountCalculationResponse> CalculateAsync(
        int userId,
        string code,
        decimal subtotal,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Discount code is required.");

        if (subtotal <= 0)
            throw new ArgumentException("Order subtotal must be greater than zero.");

        var normalizedCode = NormalizeCode(code);

        var voucher = await _userVoucherRepository.GetByCodeAsync(
            normalizedCode,
            ct);

        if (voucher is not null)
        {
            if (voucher.UserId != userId)
                throw new ArgumentException(
                    "This voucher does not belong to the current user.");

            if (!voucher.IsActive)
                throw new ArgumentException("This voucher is inactive.");

            if (voucher.IsUsed)
                throw new ArgumentException("This voucher has already been used.");

            return Calculate(
                voucher.Code,
                voucher.Type,
                voucher.Value,
                voucher.StartDate,
                voucher.EndDate,
                voucher.MinimumOrderAmount,
                voucher.MaximumDiscountAmount,
                subtotal);
        }

        var discount = await _repository.GetByCodeAsync(normalizedCode, ct);

        if (discount is null)
            throw new ArgumentException("Discount code does not exist.");

        if (!discount.IsActive)
            throw new ArgumentException("This discount is inactive.");

        return Calculate(
            discount.Code,
            discount.Type,
            discount.Value,
            discount.StartDate,
            discount.EndDate,
            discount.MinimumOrderAmount,
            discount.MaximumDiscountAmount,
            subtotal);
    }

    private static DiscountCalculationResponse Calculate(
        string code,
        DiscountType type,
        decimal value,
        DateTime startDate,
        DateTime endDate,
        decimal minimumOrderAmount,
        decimal maximumDiscountAmount,
        decimal subtotal)
    {
        var now = DateTime.UtcNow;

        if (now < startDate || now > endDate)
            throw new ArgumentException("This discount is expired or not yet valid.");

        if (subtotal < minimumOrderAmount)
            throw new ArgumentException(
                $"Minimum order amount is {minimumOrderAmount}.");

        decimal discountAmount = type switch
        {
            DiscountType.Percentage => subtotal * value / 100m,
            DiscountType.FixedAmount => value,
            _ => throw new ArgumentException("Invalid discount type.")
        };

        discountAmount = Math.Min(discountAmount, maximumDiscountAmount);
        discountAmount = Math.Min(discountAmount, subtotal);
        discountAmount = decimal.Round(
            discountAmount,
            2,
            MidpointRounding.AwayFromZero);

        return new DiscountCalculationResponse(
            code,
            type,
            subtotal,
            discountAmount,
            subtotal - discountAmount);
    }
}
