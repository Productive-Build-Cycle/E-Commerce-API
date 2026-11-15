using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class UserVoucher
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Code { get; set; } = null!;

    public DiscountType Type { get; set; }

    public decimal Value { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal MinimumOrderAmount { get; set; }

    public decimal MaximumDiscountAmount { get; set; }

    public bool IsActive { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public User User { get; set; } = null!;
}
