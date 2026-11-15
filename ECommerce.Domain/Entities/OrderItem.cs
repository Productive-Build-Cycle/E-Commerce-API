using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public static OrderItem FromProduct(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (!product.IsActive)
            throw new ArgumentException($"Product {product.Id} is not available.");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required.");

        if (product.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero.");

        return new OrderItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = decimal.Round(product.Price, 2, MidpointRounding.AwayFromZero),
            Quantity = quantity
        };
    }
}
