using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Shopping cart use cases for the authenticated customer.
/// Every method takes the user id resolved from the JWT <c>NameIdentifier</c> claim
/// (never from the request payload), which is what enforces ownership.
/// </summary>
public interface ICartService
{
    /// <summary>Returns the caller's cart; an empty cart is returned as a stable empty response (never 404).</summary>
    Task<CartResponse> GetMyCartAsync(int userId, CancellationToken ct = default);

    /// <summary>Adds a product, merging quantities when the product is already in the cart.</summary>
    Task<CartResponse> AddItemAsync(int userId, AddCartItemRequest request, CancellationToken ct = default);

    /// <summary>Replaces the quantity of the cart line identified by <paramref name="cartItemId"/>.</summary>
    Task<CartResponse> UpdateItemQuantityAsync(int userId, int cartItemId, UpdateCartItemRequest request, CancellationToken ct = default);

    /// <summary>Removes the cart line identified by <paramref name="cartItemId"/>.</summary>
    Task<CartResponse> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default);

    /// <summary>Removes every line of the caller's cart (idempotent, returns the empty cart).</summary>
    Task<CartResponse> ClearAsync(int userId, CancellationToken ct = default);
}
