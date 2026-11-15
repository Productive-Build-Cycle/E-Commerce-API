using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

/// <summary>
/// Persistence contract for the shopping cart. Implementations must eagerly load the data the
/// cart service needs (items + product, product + category) to avoid N+1 queries.
/// </summary>
public interface ICartRepository
{
    /// <summary>Cart of a user including items (and each item's product), or null when the user has no cart yet.</summary>
    Task<Cart?> GetByUserIdWithItemsAsync(int userId, bool tracking, CancellationToken ct = default);

    /// <summary>Single cart line identified by cart id + cart item id (ownership-safe), or null.</summary>
    Task<CartItem?> GetItemByIdAsync(int cartId, int cartItemId, bool tracking, CancellationToken ct = default);

    /// <summary>Single cart line identified by cart id + product id, or null.</summary>
    Task<CartItem?> GetItemAsync(int cartId, int productId, bool tracking, CancellationToken ct = default);

    /// <summary>Product including its category (may be null only for a broken FK), or null when it does not exist. Read-only.</summary>
    Task<Product?> GetProductAsync(int productId, CancellationToken ct = default);

    Task AddCartAsync(Cart cart, CancellationToken ct = default);

    Task AddItemAsync(CartItem item, CancellationToken ct = default);

    void RemoveItem(CartItem item);

    void RemoveItems(IEnumerable<CartItem> items);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
