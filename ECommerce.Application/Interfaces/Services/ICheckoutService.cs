using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Interfaces.Services;

public interface ICheckoutService
{
    Task<CheckoutResponse> CheckoutAsync(
        int userId,
        CheckoutRequest request,
        CancellationToken ct = default);
}
