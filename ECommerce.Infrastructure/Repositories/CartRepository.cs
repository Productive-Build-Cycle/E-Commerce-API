using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class CartRepository(ApplicationDbContext context) : ICartRepository
{
    private readonly ApplicationDbContext _context = context;

    public Task<Cart?> GetByUserIdWithItemsAsync(int userId, bool tracking, CancellationToken ct = default)
    {
        var query = _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product);
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public Task<CartItem?> GetItemByIdAsync(int cartId, int cartItemId, bool tracking, CancellationToken ct = default)
    {
        var query = _context.CartItems.Include(i => i.Product);
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(i => i.CartId == cartId && i.Id == cartItemId, ct);
    }

    public Task<CartItem?> GetItemAsync(int cartId, int productId, bool tracking, CancellationToken ct = default)
    {
        var query = _context.CartItems.Include(i => i.Product);
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId, ct);
    }

    public Task<Product?> GetProductAsync(int productId, CancellationToken ct = default) =>
        _context.Products.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == productId, ct);

    public async Task AddCartAsync(Cart cart, CancellationToken ct = default) => await _context.Carts.AddAsync(cart, ct);
    public async Task AddItemAsync(CartItem item, CancellationToken ct = default) => await _context.CartItems.AddAsync(item, ct);
    public void RemoveItem(CartItem item) => _context.CartItems.Remove(item);
    public void RemoveItems(IEnumerable<CartItem> items) => _context.CartItems.RemoveRange(items);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
