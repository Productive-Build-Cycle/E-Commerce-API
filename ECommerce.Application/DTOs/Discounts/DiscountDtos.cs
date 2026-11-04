using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Discounts;

public sealed record DiscountResponse(
    int Id,
    string Code,
    DiscountType Type,
    decimal Value,
    DateTime StartDate,
    DateTime EndDate,
    decimal MinimumOrderAmount,
    decimal MaximumDiscountAmount,
    bool IsActive);

public sealed record DiscountCalculationResponse(
    string Code,
    DiscountType Type,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal FinalTotal);

public class CreateDiscountRequest
{

    [Required, StringLength(50, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DiscountType Type { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Value { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal MinimumOrderAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal MaximumDiscountAmount { get; set; }
}

public class UpdateDiscountRequest : CreateDiscountRequest
{
}
