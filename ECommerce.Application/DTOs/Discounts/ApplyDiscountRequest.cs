using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Discounts;

public sealed class ApplyDiscountRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}
