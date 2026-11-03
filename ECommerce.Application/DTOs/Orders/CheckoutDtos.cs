using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.DTOs.Orders;

public sealed class CheckoutRequest
{
    public string? DiscountCode { get; set; }
}

public sealed record CheckoutResponse(
    int OrderId,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal FinalTotal,
    string Status);
