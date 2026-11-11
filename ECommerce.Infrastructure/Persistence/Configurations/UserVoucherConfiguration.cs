using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public sealed class UserVoucherConfiguration : IEntityTypeConfiguration<UserVoucher>
{
    public void Configure(EntityTypeBuilder<UserVoucher> builder)
    {
        builder.ToTable("UserVouchers", table =>
        {
            table.HasCheckConstraint(
                "CK_UserVouchers_Value_Positive",
                "[Value] > 0");

            table.HasCheckConstraint(
                "CK_UserVouchers_DateRange",
                "[StartDate] < [EndDate]");

            table.HasCheckConstraint(
                "CK_UserVouchers_MinimumOrderAmount",
                "[MinimumOrderAmount] >= 0");

            table.HasCheckConstraint(
                "CK_UserVouchers_MaximumDiscountAmount",
                "[MaximumDiscountAmount] >= 0");

            table.HasCheckConstraint(
                "CK_UserVouchers_Type",
                "[Type] IN (1, 2)");

            table.HasCheckConstraint(
                "CK_UserVouchers_UsedAtUtc",
                "([IsUsed] = 0 AND [UsedAtUtc] IS NULL) OR " +
                "([IsUsed] = 1 AND [UsedAtUtc] IS NOT NULL)");

            table.HasCheckConstraint(
                "CK_UserVouchers_PercentageValue",
                "[Type] <> 1 OR [Value] <= 100");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.MinimumOrderAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.MaximumDiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Vouchers)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
