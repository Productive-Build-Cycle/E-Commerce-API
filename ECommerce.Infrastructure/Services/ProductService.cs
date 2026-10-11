using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    public ProductService(IProductRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ProductResponse>> ListAdminAsync(CancellationToken ct = default) =>
        (await _repository.ListAdminAsync(ct)).Select(Map).ToList();
    public async Task<IReadOnlyList<ProductResponse>> ListActiveAsync(int? categoryId = null, CancellationToken ct = default) =>
        (await _repository.ListActiveAsync(categoryId, ct)).Select(Map).ToList();
    public async Task<ProductResponse?> GetAdminByIdAsync(int id, CancellationToken ct = default) => Map(await _repository.GetByIdAsync(id, ct));
    public async Task<ProductResponse?> GetCustomerByIdAsync(int id, CancellationToken ct = default) => Map(await _repository.GetActiveByIdAsync(id, ct));

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        Validate(request.Price, request.StockQuantity);
        await ValidateCategory(request.CategoryId, ct);
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description ?? string.Empty,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            IsActive = false
        };
        await _repository.AddAsync(product, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(await _repository.GetByIdAsync(product.Id, ct))!;
    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default)
    {
        Validate(request.Price, request.StockQuantity);
        await ValidateCategory(request.CategoryId, ct);
        var product = await _repository.GetByIdAsync(id, ct);
        if (product is null) return null;
        product.Name = request.Name.Trim(); product.Description = request.Description ?? string.Empty;
        product.Price = request.Price; product.StockQuantity = request.StockQuantity; product.CategoryId = request.CategoryId;
        await _repository.SaveChangesAsync(ct);
        return Map(await _repository.GetByIdAsync(id, ct));
    }

    public async Task<ProductResponse?> SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product is null) return null;
        if (active) await ValidateCategory(product.CategoryId, ct);
        product.IsActive = active;
        await _repository.SaveChangesAsync(ct);
        return Map(await _repository.GetByIdAsync(id, ct));
    }

    public async Task<ProductResponse?> UpdateStockAsync(int id, int stockQuantity, CancellationToken ct = default)
    {
        if (stockQuantity < 0) throw new ArgumentException("Stock quantity cannot be negative.");
        var product = await _repository.GetByIdAsync(id, ct);
        if (product is null) return null;
        product.StockQuantity = stockQuantity;
        await _repository.SaveChangesAsync(ct);
        return Map(await _repository.GetByIdAsync(id, ct));
    }

    private async Task ValidateCategory(int id, CancellationToken ct)
    {
        if (!await _repository.ActiveCategoryExistsAsync(id, ct)) throw new ArgumentException("Category does not exist or is inactive.");
    }
    private static void Validate(decimal price, int stock)
    {
        if (price <= 0) throw new ArgumentException("Price must be greater than zero.");
        if (stock < 0) throw new ArgumentException("Stock quantity cannot be negative.");
    }
    private static ProductResponse? Map(Product? p) => p is null ? null :
        new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.IsActive, p.CategoryId, p.Category?.Name ?? string.Empty);
}
