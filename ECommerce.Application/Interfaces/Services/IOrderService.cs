using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(int userId, CreateOrderRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<OrderResponse>> ListForCustomerAsync(int userId, CancellationToken ct = default);
    Task<OrderResponse?> GetForCustomerAsync(int userId, int orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderResponse>> ListForAdminAsync(CancellationToken ct = default);
    Task<OrderResponse?> GetForAdminAsync(int orderId, CancellationToken ct = default);
    Task<OrderResponse?> ChangeStatusAsync(int orderId, OrderStatus status, CancellationToken ct = default);
}
