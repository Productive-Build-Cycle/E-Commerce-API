using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        if (StockQuantity < quantity)
            throw new ArgumentException($"Insufficient stock for '{Name}'.");

        StockQuantity -= quantity;
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        StockQuantity += quantity;
    }
}
