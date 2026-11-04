
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.SeedData;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(cancellationToken);
        await SeedCategoriesAsync(cancellationToken);
        await SeedProductsAsync(cancellationToken);
        await SeedCartsAsync(cancellationToken);
        await SeedDiscountsAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var users = new[]
        {
            new
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                Password = "Admin@123456",
                Role = UserRole.Admin
            },
            new
            {
                FirstName = "John",
                LastName = "Customer",
                Email = "john@example.com",
                Password = "Customer@123456",
                Role = UserRole.Customer
            },
            new
            {
                FirstName = "Jane",
                LastName = "Customer",
                Email = "jane@example.com",
                Password = "Customer@123456",
                Role = UserRole.Customer
            }
        };

        foreach (var item in users)
        {
            var exists = await _context.Users
                .AnyAsync(x => x.Email == item.Email, cancellationToken);

            if (exists)
                continue;

            _context.Users.Add(new User
            {
                FirstName = item.FirstName,
                LastName = item.LastName,
                Email = item.Email,
                PasswordHash = _passwordHasher.Hash(item.Password),
                Role = item.Role
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = new[]
        {
            "Electronics",
            "Accessories"
        };

        foreach (var name in categories)
        {
            var exists = await _context.Categories
                .AnyAsync(x => x.Name == name, cancellationToken);

            if (exists)
                continue;

            _context.Categories.Add(new Category
            {
                Name = name,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedProductsAsync(CancellationToken cancellationToken)
    {
        var electronics = await _context.Categories
            .FirstAsync(x => x.Name == "Electronics", cancellationToken);

        var accessories = await _context.Categories
            .FirstAsync(x => x.Name == "Accessories", cancellationToken);

        var products = new[]
        {
            new Product
            {
                Name = "Laptop",
                Description = "Development laptop",
                Price = 1200m,
                StockQuantity = 10,
                IsActive = true,
                CategoryId = electronics.Id
            },
            new Product
            {
                Name = "Smartphone",
                Description = "Modern smartphone",
                Price = 800m,
                StockQuantity = 15,
                IsActive = true,
                CategoryId = electronics.Id
            },
            new Product
            {
                Name = "Keyboard",
                Description = "Mechanical keyboard",
                Price = 100m,
                StockQuantity = 20,
                IsActive = true,
                CategoryId = accessories.Id
            },
            new Product
            {
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 50m,
                StockQuantity = 30,
                IsActive = true,
                CategoryId = accessories.Id
            }
        };

        foreach (var product in products)
        {
            var exists = await _context.Products
                .AnyAsync(x => x.Name == product.Name, cancellationToken);

            if (exists)
                continue;

            _context.Products.Add(product);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCartsAsync(CancellationToken cancellationToken)
    {
        var customers = await _context.Users
            .Where(x => x.Role == UserRole.Customer)
            .ToListAsync(cancellationToken);

        var products = await _context.Products
            .Where(x => x.Name == "Laptop" ||
                        x.Name == "Keyboard" ||
                        x.Name == "Mouse")
            .ToDictionaryAsync(x => x.Name, cancellationToken);

        foreach (var customer in customers)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(
                    x => x.UserId == customer.Id,
                    cancellationToken);

            if (cart is null)
            {
                cart = new Cart
                {
                    UserId = customer.Id
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var items = customer.Email == "john@example.com"
                ? new[] { ("Laptop", 1), ("Keyboard", 2) }
                : new[] { ("Mouse", 2), ("Keyboard", 1) };

            foreach (var (productName, quantity) in items)
            {
                var product = products[productName];

                var exists = await _context.CartItems.AnyAsync(
                    x => x.CartId == cart.Id &&
                         x.ProductId == product.Id,
                    cancellationToken);

                if (exists)
                    continue;

                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = quantity
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDiscountsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var discounts = new[]
        {
            new Discount
            {
                Code = "WELCOME20",
                Type = DiscountType.Percentage,
                Value = 20m,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(30),
                MinimumOrderAmount = 100m,
                MaximumDiscountAmount = 200m,
                IsActive = true
            },
            new Discount
            {
                Code = "FIXED50",
                Type = DiscountType.FixedAmount,
                Value = 50m,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(30),
                MinimumOrderAmount = 100m,
                MaximumDiscountAmount = 50m,
                IsActive = true
            },
            new Discount
            {
                Code = "INACTIVE10",
                Type = DiscountType.Percentage,
                Value = 10m,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(30),
                MinimumOrderAmount = 0m,
                MaximumDiscountAmount = 100m,
                IsActive = false
            },
            new Discount
            {
                Code = "EXPIRED10",
                Type = DiscountType.Percentage,
                Value = 10m,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(-1),
                MinimumOrderAmount = 0m,
                MaximumDiscountAmount = 100m,
                IsActive = true
            },
            new Discount
            {
                Code = "FUTURE10",
                Type = DiscountType.Percentage,
                Value = 10m,
                StartDate = now.AddDays(5),
                EndDate = now.AddDays(30),
                MinimumOrderAmount = 0m,
                MaximumDiscountAmount = 100m,
                IsActive = true
            }
        };

        foreach (var discount in discounts)
        {
            var exists = await _context.Discounts
                .AnyAsync(x => x.Code == discount.Code, cancellationToken);

            if (exists)
                continue;

            _context.Discounts.Add(discount);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
