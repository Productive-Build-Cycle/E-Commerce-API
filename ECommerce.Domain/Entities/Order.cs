using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public OrderStatus Status { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalTotal { get; set; }

    public User User { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public static Order Place(int userId, IReadOnlyCollection<OrderItem> items, decimal discountAmount)
    {
        if (userId <= 0)
            throw new ArgumentException("User is required.");

        if (items is null || items.Count == 0)
            throw new ArgumentException("Order must contain at least one item.");

        foreach (var item in items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            if (item.UnitPrice <= 0)
                throw new ArgumentException("Unit price must be greater than zero.");

            if (string.IsNullOrWhiteSpace(item.ProductName))
                throw new ArgumentException("Product name is required.");
        }

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative.");

        var subtotal = RoundMoney(items.Sum(item => item.UnitPrice * item.Quantity));
        discountAmount = RoundMoney(discountAmount);

        if (discountAmount > subtotal)
            throw new ArgumentException("Discount amount cannot exceed the order subtotal.");

        var finalTotal = subtotal - discountAmount;
        if (finalTotal < 0)
            throw new ArgumentException("Final total cannot be negative.");

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            FinalTotal = finalTotal
        };

        foreach (var item in items)
            order.Items.Add(item);

        return order;
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (CanTransitionTo(newStatus))
        {
            Status = newStatus;
            return;
        }

        var allowed = AllowedFrom(Status);
        if (allowed.Count == 0)
            throw new ArgumentException($"Order status cannot change once it is {Status}.");

        throw new ArgumentException(
            $"Cannot change order status from {Status} to {newStatus}. Allowed: {string.Join(", ", allowed)}.");
    }

    public bool CanTransitionTo(OrderStatus newStatus) => AllowedFrom(Status).Contains(newStatus);

    private static IReadOnlyList<OrderStatus> AllowedFrom(OrderStatus status) => status switch
    {
        OrderStatus.Pending => [OrderStatus.Paid, OrderStatus.Cancelled],
        OrderStatus.Paid => [OrderStatus.Processing, OrderStatus.Cancelled],
        OrderStatus.Processing => [OrderStatus.Shipped, OrderStatus.Cancelled],
        OrderStatus.Shipped => [OrderStatus.Delivered],
        _ => []
    };

    private static decimal RoundMoney(decimal amount) =>
        decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
}
