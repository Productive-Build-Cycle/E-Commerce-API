using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public sealed class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts", table =>
        {
            table.HasCheckConstraint(
                "CK_Discounts_Value_Positive",
                "[Value] > 0");

            table.HasCheckConstraint(
                "CK_Discounts_DateRange",
                "[StartDate] < [EndDate]");

            table.HasCheckConstraint(
                "CK_Discounts_MinimumOrderAmount",
                "[MinimumOrderAmount] >= 0");

            table.HasCheckConstraint(
                "CK_Discounts_MaximumDiscountAmount",
                "[MaximumDiscountAmount] >= 0");

            table.HasCheckConstraint(
                "CK_Discounts_Type",
                "[Type] IN (1, 2)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
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
    }
}
