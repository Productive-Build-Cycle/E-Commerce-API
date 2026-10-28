using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Cart;

/// <summary>
/// One line of the customer cart. The route key for update/remove is <see cref="CartItemId"/>
/// (never the product id, because the same product can never appear twice in one cart).
/// </summary>
public sealed record CartItemResponse(
    int CartItemId,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>
/// The caller's cart with all items and the grand total.
/// A customer without a persisted cart gets <see cref="CartId"/> = 0 and an empty <see cref="Items"/> list,
/// so "empty cart" is a stable 200 response and never a 404.
/// </summary>
public sealed record CartResponse(int CartId, IReadOnlyList<CartItemResponse> Items, decimal Total, int TotalQuantity)
{
    /// <summary>Stable empty-cart payload (used when no cart row exists yet or the cart was just emptied).</summary>
    public static CartResponse Empty(int cartId = 0) =>
        new(cartId, Array.Empty<CartItemResponse>(), 0m, 0);
}

/// <summary>Adds a product to the authenticated customer cart (merges with an existing line).</summary>
public sealed class AddCartItemRequest
{
    /// <summary>Product to add. Must exist, be active and its category must be active.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
    public int ProductId { get; set; }

    /// <summary>Quantity to add; the resulting (merged) line quantity must not exceed the current stock.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }
}

/// <summary>Sets (replaces) the absolute quantity of an existing cart line.</summary>
public sealed class UpdateCartItemRequest
{
    /// <summary>New absolute line quantity; must be greater than zero and not exceed the current stock.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }
}

/// <summary>Business error categories reported by the cart service; the controller maps them to HTTP status codes.</summary>
public enum CartErrorKind
{
    /// <summary>Cart item or product does not exist (HTTP 404).</summary>
    NotFound,

    /// <summary>Requested quantity is invalid, e.g. &lt;= 0 or overflow (HTTP 400).</summary>
    InvalidQuantity,

    /// <summary>Product (or its category) is inactive and cannot be purchased (HTTP 409).</summary>
    ProductInactive,

    /// <summary>Requested (merged) quantity exceeds the current product stock (HTTP 409).</summary>
    InsufficientStock,

    /// <summary>Concurrent request inserted the same product twice; the unique index rejected it (HTTP 409).</summary>
    Duplicate
}

/// <summary>
/// Thrown when a cart business rule is violated. Carries only a safe, client-facing message;
/// the controller maps <see cref="ErrorKind"/> to HTTP 400 / 404 / 409 and never leaks exception internals.
/// </summary>
public sealed class CartBusinessException : Exception
{
    public CartErrorKind ErrorKind { get; }

    public CartBusinessException(CartErrorKind kind, string message) : base(message) => ErrorKind = kind;

    public CartBusinessException(CartErrorKind kind, string message, Exception innerException)
        : base(message, innerException) => ErrorKind = kind;
}
