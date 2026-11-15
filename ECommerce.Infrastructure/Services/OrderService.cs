using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    public OrderService(IOrderRepository repository) => _repository = repository;

    public async Task<OrderResponse> CreateAsync(int userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId <= 0)
            throw new ArgumentException("User is required.");

        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException("Order must contain at least one item.");

        var duplicate = request.Items
            .GroupBy(item => item.ProductId)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
            throw new ArgumentException($"Product {duplicate.Key} can only appear once on an order.");

        foreach (var item in request.Items)
        {
            if (item.ProductId <= 0)
                throw new ArgumentException("Product is required.");

            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (!await _repository.UserExistsAsync(userId, ct))
            throw new ArgumentException("User does not exist.");

        var productIds = request.Items.Select(item => item.ProductId).ToArray();
        var products = await _repository.GetProductsByIdsAsync(productIds, ct);
        var productsById = products.ToDictionary(product => product.Id);

        var snapshots = new List<OrderItem>(request.Items.Count);
        foreach (var line in request.Items)
        {
            if (!productsById.TryGetValue(line.ProductId, out var product) ||
                !product.IsActive ||
                product.Category is not { IsActive: true })
            {
                throw new ArgumentException($"Product {line.ProductId} is not available.");
            }

            if (product.StockQuantity < line.Quantity)
                throw new ArgumentException($"Insufficient stock for '{product.Name}'.");

            snapshots.Add(OrderItem.FromProduct(product, line.Quantity));
        }

        var order = Order.Place(userId, snapshots, request.DiscountAmount);

        foreach (var line in request.Items)
            productsById[line.ProductId].ReserveStock(line.Quantity);

        await _repository.AddAsync(order, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> ListForCustomerAsync(int userId, CancellationToken ct = default) =>
        (await _repository.ListByUserAsync(userId, ct)).Select(Map).ToList();

    public async Task<OrderResponse?> GetForCustomerAsync(int userId, int orderId, CancellationToken ct = default) =>
        MapOrNull(await _repository.GetByIdForUserAsync(orderId, userId, ct));

    public async Task<IReadOnlyList<OrderResponse>> ListForAdminAsync(CancellationToken ct = default) =>
        (await _repository.ListAsync(ct)).Select(Map).ToList();

    public async Task<OrderResponse?> GetForAdminAsync(int orderId, CancellationToken ct = default) =>
        MapOrNull(await _repository.GetByIdAsync(orderId, ct));

    public async Task<OrderResponse?> ChangeStatusAsync(int orderId, OrderStatus status, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentException("Order status is not valid.");

        var order = await _repository.GetByIdAsync(orderId, ct);
        if (order is null)
            return null;

        var previous = order.Status;
        order.ChangeStatus(status);

        if (status == OrderStatus.Cancelled && previous != OrderStatus.Cancelled)
            await ReleaseStockAsync(order, ct);

        await _repository.SaveChangesAsync(ct);
        return Map(order);
    }

    private async Task ReleaseStockAsync(Order order, CancellationToken ct)
    {
        var productIds = order.Items.Select(item => item.ProductId).Distinct().ToArray();
        var products = await _repository.GetProductsByIdsAsync(productIds, ct);
        var productsById = products.ToDictionary(product => product.Id);

        foreach (var item in order.Items)
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
                throw new ArgumentException($"Product {item.ProductId} is not available.");

            product.ReleaseStock(item.Quantity);
        }
    }

    private static OrderResponse? MapOrNull(Order? order) => order is null ? null : Map(order);

    private static OrderResponse Map(Order order) =>
        new(
            order.Id,
            order.UserId,
            order.Status,
            order.Subtotal,
            order.DiscountAmount,
            order.FinalTotal,
            order.Items.Select(MapItem).ToList());

    private static OrderItemResponse MapItem(OrderItem item) =>
        new(item.Id, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.UnitPrice * item.Quantity);
}
