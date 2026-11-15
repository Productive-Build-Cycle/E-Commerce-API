using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Services;

namespace ECommerce.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task Create_stores_purchase_price_discount_and_final_total()
    {
        var repository = new FakeOrderRepository();
        var pen = AvailableProduct(id: 1, name: "Pen", price: 12.50m, stock: 5);
        var notebook = AvailableProduct(id: 2, name: "Notebook", price: 3.25m, stock: 4);
        repository.Products.Add(pen);
        repository.Products.Add(notebook);
        repository.UserIds.Add(7);
        var service = new OrderService(repository);

        var created = await service.CreateAsync(7, new CreateOrderRequest
        {
            DiscountAmount = 4m,
            Items =
            [
                new CreateOrderItemRequest { ProductId = 1, Quantity = 2 },
                new CreateOrderItemRequest { ProductId = 2, Quantity = 4 }
            ]
        });

        pen.Price = 99m;
        pen.Name = "Marker";

        var stored = await service.GetForCustomerAsync(7, created.Id);
        Assert.NotNull(stored);
        Assert.Equal(38m, stored.Subtotal);
        Assert.Equal(4m, stored.DiscountAmount);
        Assert.Equal(34m, stored.FinalTotal);
        Assert.Equal(OrderStatus.Pending, stored.Status);

        var storedPen = Assert.Single(stored.Items, item => item.ProductId == 1);
        Assert.Equal("Pen", storedPen.ProductName);
        Assert.Equal(12.50m, storedPen.UnitPrice);
        Assert.Equal(25m, storedPen.LineTotal);
        Assert.Equal(3, pen.StockQuantity);
        Assert.Equal(0, notebook.StockQuantity);
    }

    [Fact]
    public async Task Create_rejects_a_discount_above_the_subtotal_without_taking_stock()
    {
        var repository = new FakeOrderRepository();
        var pen = AvailableProduct(id: 1, name: "Pen", price: 10m, stock: 5);
        repository.Products.Add(pen);
        repository.UserIds.Add(7);
        var service = new OrderService(repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(7, new CreateOrderRequest
        {
            DiscountAmount = 20.01m,
            Items = [new CreateOrderItemRequest { ProductId = 1, Quantity = 2 }]
        }));

        Assert.Contains("cannot exceed the order subtotal", exception.Message);
        Assert.Equal(5, pen.StockQuantity);
        Assert.Empty(repository.Orders);
    }

    [Fact]
    public async Task Create_rejects_insufficient_stock()
    {
        var repository = new FakeOrderRepository();
        repository.Products.Add(AvailableProduct(id: 1, name: "Pen", price: 10m, stock: 1));
        repository.UserIds.Add(7);
        var service = new OrderService(repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(7, new CreateOrderRequest
        {
            Items = [new CreateOrderItemRequest { ProductId = 1, Quantity = 2 }]
        }));

        Assert.Contains("Insufficient stock", exception.Message);
        Assert.Empty(repository.Orders);
    }

    [Fact]
    public async Task Customer_can_view_only_their_own_orders()
    {
        var repository = new FakeOrderRepository();
        repository.Products.Add(AvailableProduct(id: 1, name: "Pen", price: 10m, stock: 10));
        repository.UserIds.Add(7);
        repository.UserIds.Add(8);
        var service = new OrderService(repository);

        var own = await service.CreateAsync(7, OrderFor(1));
        var other = await service.CreateAsync(8, OrderFor(1));

        var ownList = await service.ListForCustomerAsync(7);
        var ownDetails = await service.GetForCustomerAsync(7, own.Id);
        var hidden = await service.GetForCustomerAsync(7, other.Id);

        Assert.Equal(own.Id, Assert.Single(ownList).Id);
        Assert.NotNull(ownDetails);
        Assert.Equal(7, ownDetails.UserId);
        Assert.Null(hidden);
    }

    [Fact]
    public async Task Admin_can_view_every_order_and_only_valid_status_changes_are_saved()
    {
        var repository = new FakeOrderRepository();
        var pen = AvailableProduct(id: 1, name: "Pen", price: 10m, stock: 4);
        repository.Products.Add(pen);
        repository.UserIds.Add(7);
        var service = new OrderService(repository);
        var created = await service.CreateAsync(7, OrderFor(1));

        var all = await service.ListForAdminAsync();
        var details = await service.GetForAdminAsync(created.Id);
        Assert.Equal(created.Id, Assert.Single(all).Id);
        Assert.NotNull(details);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ChangeStatusAsync(created.Id, OrderStatus.Shipped));
        Assert.Contains("Cannot change order status", exception.Message);

        var unchanged = await service.GetForAdminAsync(created.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(OrderStatus.Pending, unchanged.Status);

        var paid = await service.ChangeStatusAsync(created.Id, OrderStatus.Paid);
        Assert.NotNull(paid);
        Assert.Equal(OrderStatus.Paid, paid.Status);

        var cancelled = await service.ChangeStatusAsync(created.Id, OrderStatus.Cancelled);
        Assert.NotNull(cancelled);
        Assert.Equal(OrderStatus.Cancelled, cancelled.Status);
        Assert.Equal(4, pen.StockQuantity);
    }

    private static CreateOrderRequest OrderFor(int productId) => new()
    {
        Items = [new CreateOrderItemRequest { ProductId = productId, Quantity = 1 }]
    };

    private static Product AvailableProduct(int id, string name, decimal price, int stock) => new()
    {
        Id = id,
        Name = name,
        Description = name,
        Price = price,
        StockQuantity = stock,
        IsActive = true,
        CategoryId = 3,
        Category = new Category { Id = 3, Name = "Stationery", IsActive = true }
    };

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; } = [];
        public List<Product> Products { get; } = [];
        public HashSet<int> UserIds { get; } = [];
        private int _nextOrderId = 1;
        private int _nextItemId = 1;

        public Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Order>>(Orders.OrderByDescending(order => order.Id).ToList());

        public Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Order>>(
                Orders.Where(order => order.UserId == userId).OrderByDescending(order => order.Id).ToList());

        public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Orders.FirstOrDefault(order => order.Id == id));

        public Task<Order?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Orders.FirstOrDefault(order => order.Id == id && order.UserId == userId));

        public Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(UserIds.Contains(userId));

        public Task<IReadOnlyList<Product>> GetProductsByIdsAsync(
            IReadOnlyCollection<int> productIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Product>>(Products.Where(product => productIds.Contains(product.Id)).ToList());

        public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            Orders.Add(order);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var order in Orders.Where(order => order.Id == 0))
            {
                order.Id = _nextOrderId++;
                foreach (var item in order.Items)
                    item.Id = _nextItemId++;
            }

            return Task.CompletedTask;
        }
    }
}
