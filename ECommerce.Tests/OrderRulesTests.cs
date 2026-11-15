using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Tests;

public class OrderRulesTests
{
    [Fact]
    public void Place_calculates_subtotal_from_purchase_prices_and_stores_discount()
    {
        var pen = Product("Pen", 12.50m, stock: 5);
        var notebook = Product("Notebook", 3.25m, stock: 10, id: 2);

        var order = Order.Place(
            userId: 7,
            [
                OrderItem.FromProduct(pen, 2),
                OrderItem.FromProduct(notebook, 4)
            ],
            discountAmount: 4.00m);

        pen.Price = 99m;
        pen.Name = "Marker";

        var penLine = Assert.Single(order.Items, item => item.ProductId == pen.Id);
        Assert.Equal("Pen", penLine.ProductName);
        Assert.Equal(12.50m, penLine.UnitPrice);
        Assert.Equal(38.00m, order.Subtotal);
        Assert.Equal(4.00m, order.DiscountAmount);
        Assert.Equal(34.00m, order.FinalTotal);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Place_allows_a_zero_final_total()
    {
        var order = Order.Place(4, [OrderItem.FromProduct(Product("Pen", 10m, 1), 1)], 10m);

        Assert.Equal(10m, order.Subtotal);
        Assert.Equal(10m, order.DiscountAmount);
        Assert.Equal(0m, order.FinalTotal);
    }

    [Fact]
    public void Place_rejects_a_discount_that_would_make_the_final_total_negative()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Order.Place(4, [OrderItem.FromProduct(Product("Pen", 10m, 1), 2)], 20.01m));

        Assert.Contains("cannot exceed the order subtotal", exception.Message);
    }

    [Fact]
    public void Place_rejects_a_negative_discount()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Order.Place(4, [OrderItem.FromProduct(Product("Pen", 10m, 1), 1)], -0.01m));

        Assert.Contains("cannot be negative", exception.Message);
    }

    [Fact]
    public void Status_moves_only_through_the_fulfillment_sequence()
    {
        var order = Order.Place(4, [OrderItem.FromProduct(Product("Pen", 10m, 1), 1)], 0m);

        order.ChangeStatus(OrderStatus.Paid);
        order.ChangeStatus(OrderStatus.Processing);
        order.ChangeStatus(OrderStatus.Shipped);
        order.ChangeStatus(OrderStatus.Delivered);

        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Throws<ArgumentException>(() => order.ChangeStatus(OrderStatus.Cancelled));
    }

    [Fact]
    public void Status_can_be_cancelled_before_shipment_and_not_after()
    {
        var pending = Order.Place(4, [OrderItem.FromProduct(Product("Pen", 10m, 1), 1)], 0m);
        pending.ChangeStatus(OrderStatus.Cancelled);
        Assert.Equal(OrderStatus.Cancelled, pending.Status);

        var shipped = Order.Place(4, [OrderItem.FromProduct(Product("Pen", 10m, 1), 1)], 0m);
        shipped.ChangeStatus(OrderStatus.Paid);
        shipped.ChangeStatus(OrderStatus.Processing);
        shipped.ChangeStatus(OrderStatus.Shipped);

        var exception = Assert.Throws<ArgumentException>(() => shipped.ChangeStatus(OrderStatus.Cancelled));
        Assert.Equal(OrderStatus.Shipped, shipped.Status);
        Assert.Contains("Delivered", exception.Message);
    }

    [Fact]
    public void Status_rejects_skipped_transitions()
    {
        var order = Order.Place(4, [OrderItem.FromProduct(Product("Pen", 10m, 1), 1)], 0m);

        var exception = Assert.Throws<ArgumentException>(() => order.ChangeStatus(OrderStatus.Shipped));

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Contains("Paid", exception.Message);
        Assert.Contains("Cancelled", exception.Message);
    }

    private static Product Product(string name, decimal price, int stock, int id = 1) => new()
    {
        Id = id,
        Name = name,
        Price = price,
        StockQuantity = stock,
        IsActive = true,
        CategoryId = 1
    };
}
