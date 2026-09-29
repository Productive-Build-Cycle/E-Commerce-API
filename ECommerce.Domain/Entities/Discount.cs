using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Discount
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public DiscountType Type { get; set; }

    public decimal Value { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal MinimumOrderAmount { get; set; }

    public decimal MaximumDiscountAmount { get; set; }

    public bool IsActive { get; set; }
}
