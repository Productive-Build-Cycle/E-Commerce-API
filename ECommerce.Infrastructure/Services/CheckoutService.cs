using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public sealed class CheckoutService : ICheckoutService
{
    private readonly ApplicationDbContext _context;
    private readonly IDiscountService _discountService;
    private readonly IUserVoucherRepository _userVoucherRepository;

    public CheckoutService(
        ApplicationDbContext context,
        IDiscountService discountService,
        IUserVoucherRepository userVoucherRepository)
    {
        _context = context;
        _discountService = discountService;
        _userVoucherRepository = userVoucherRepository;
    }

    public async Task<CheckoutResponse> CheckoutAsync(
        int userId,
        CheckoutRequest request,
        CancellationToken ct = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(ct);

        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart is null || cart.Items.Count == 0)
                throw new ArgumentException("Your shopping cart is empty.");

            // Validate products and stock.
            foreach (var item in cart.Items)
            {
                var product = item.Product;

                if (product is null ||
                    !product.IsActive ||
                    product.Category is null ||
                    !product.Category.IsActive)
                {
                    throw new ArgumentException(
                        $"Product with ID {item.ProductId} is not available.");
                }

                if (item.Quantity <= 0 ||
                    item.Quantity > product.StockQuantity)
                {
                    throw new ArgumentException(
                        $"Insufficient stock for product {product.Name}.");
                }
            }

            // Calculate subtotal using current database prices.
            var subtotal = cart.Items.Sum(item =>
                item.Product.Price * item.Quantity);

            if (subtotal <= 0)
                throw new ArgumentException("Order subtotal must be greater than zero.");

            decimal discountAmount = 0;
            UserVoucher? voucher = null;

            // Validate and calculate discount again during checkout.
            if (!string.IsNullOrWhiteSpace(request.DiscountCode))
            {
                var discountResult = await _discountService.CalculateAsync(
                    userId,
                    request.DiscountCode,
                    subtotal,
                    ct);

                discountAmount = discountResult.DiscountAmount;

                voucher = await _userVoucherRepository.GetByCodeAsync(
                    NormalizeCode(request.DiscountCode),
                    ct);
            }

            var finalTotal = subtotal - discountAmount;

            if (finalTotal < 0)
                throw new ArgumentException("Order total cannot be negative.");

            // Create order with historical product information.
            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                FinalTotal = finalTotal,

                Items = cart.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);

            // Decrease product stock.
            foreach (var item in cart.Items)
            {
                item.Product.StockQuantity -= item.Quantity;
            }

            // Clear cart after successful order creation.
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync(ct);

            if (voucher is not null)
            {
                voucher.IsUsed = true;
                voucher.UsedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);

            return new CheckoutResponse(
                order.Id,
                order.Subtotal,
                order.DiscountAmount,
                order.FinalTotal,
                order.Status.ToString());
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(ct);

            throw new ArgumentException(
                "This voucher has already been used. Please refresh your cart and try again.");
        }
    }

    private static string NormalizeCode(string code) =>
        code.Trim().ToUpperInvariant();
}
