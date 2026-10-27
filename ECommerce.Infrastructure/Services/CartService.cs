using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Services;

public sealed class CartService(ICartRepository repository) : ICartService
{
    private readonly ICartRepository _repository = repository;

    public async Task<CartResponse> GetMyCartAsync(int userId, CancellationToken ct = default) =>
        Map(await _repository.GetByUserIdWithItemsAsync(userId, false, ct));

    public async Task<CartResponse> AddItemAsync(int userId, AddCartItemRequest request, CancellationToken ct = default)
    {
        if (request.Quantity <= 0) throw Error(CartErrorKind.InvalidQuantity, "Quantity must be greater than zero.");
        var cart = await _repository.GetByUserIdWithItemsAsync(userId, true, ct);
        if (cart is null) { cart = new Cart { UserId = userId }; await _repository.AddCartAsync(cart, ct); await _repository.SaveChangesAsync(ct); }
        var product = await _repository.GetProductAsync(request.ProductId, ct) ?? throw Error(CartErrorKind.NotFound, "Product not found.");
        EnsureAvailable(product);
        var item = await _repository.GetItemAsync(cart.Id, product.Id, true, ct);
        long quantity = (long)(item?.Quantity ?? 0) + request.Quantity;
        if (quantity > product.StockQuantity) throw Error(CartErrorKind.InsufficientStock, "Insufficient stock.");
        if (item is null) await _repository.AddItemAsync(new CartItem { CartId = cart.Id, ProductId = product.Id, Quantity = (int)quantity }, ct);
        else item.Quantity = (int)quantity;
        await _repository.SaveChangesAsync(ct);
        return await GetMyCartAsync(userId, ct);
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(int userId, int cartItemId, UpdateCartItemRequest request, CancellationToken ct = default)
    {
        if (request.Quantity <= 0) throw Error(CartErrorKind.InvalidQuantity, "Quantity must be greater than zero.");
        var cart = await _repository.GetByUserIdWithItemsAsync(userId, true, ct) ?? throw Error(CartErrorKind.NotFound, "Cart not found.");
        var item = await _repository.GetItemByIdAsync(cart.Id, cartItemId, true, ct) ?? throw Error(CartErrorKind.NotFound, "Cart item not found.");
        var product = item.Product ?? await _repository.GetProductAsync(item.ProductId, ct) ?? throw Error(CartErrorKind.NotFound, "Product not found.");
        EnsureAvailable(product);
        if (request.Quantity > product.StockQuantity) throw Error(CartErrorKind.InsufficientStock, "Insufficient stock.");
        item.Quantity = request.Quantity;
        await _repository.SaveChangesAsync(ct);
        return await GetMyCartAsync(userId, ct);
    }

    public async Task<CartResponse> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default)
    {
        var cart = await _repository.GetByUserIdWithItemsAsync(userId, true, ct) ?? throw Error(CartErrorKind.NotFound, "Cart not found.");
        var item = await _repository.GetItemByIdAsync(cart.Id, cartItemId, true, ct) ?? throw Error(CartErrorKind.NotFound, "Cart item not found.");
        _repository.RemoveItem(item);
        await _repository.SaveChangesAsync(ct);
        return await GetMyCartAsync(userId, ct);
    }

    public async Task<CartResponse> ClearAsync(int userId, CancellationToken ct = default)
    {
        var cart = await _repository.GetByUserIdWithItemsAsync(userId, true, ct);
        if (cart is null) return CartResponse.Empty();
        _repository.RemoveItems(cart.Items);
        await _repository.SaveChangesAsync(ct);
        return CartResponse.Empty(cart.Id);
    }

    private static void EnsureAvailable(Product product)
    {
        if (!product.IsActive || product.Category is null || !product.Category.IsActive)
            throw Error(CartErrorKind.ProductInactive, "Product is not available.");
    }

    private static CartBusinessException Error(CartErrorKind kind, string message) => new(kind, message);

    private static CartResponse Map(Cart? cart)
    {
        if (cart is null) return CartResponse.Empty();
        var items = cart.Items.OrderBy(i => i.Id).Select(i => new CartItemResponse(i.Id, i.ProductId,
            i.Product?.Name ?? string.Empty, i.Quantity, i.Product?.Price ?? 0m, i.Quantity * (i.Product?.Price ?? 0m))).ToList();
        return new CartResponse(cart.Id, items, items.Sum(i => i.LineTotal), items.Sum(i => i.Quantity));
    }
}
