using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Products;

public sealed record ProductResponse(int Id, string Name, string Description, decimal Price, int StockQuantity, bool IsActive, int CategoryId, string CategoryName);

public class CreateProductRequest
{
    [Required, StringLength(200, MinimumLength = 1)] public string Name { get; set; } = string.Empty;
    [StringLength(2000)] public string Description { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal Price { get; set; }
    [Range(0, int.MaxValue)] public int StockQuantity { get; set; }
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
}

public sealed class UpdateProductRequest : CreateProductRequest { }
public sealed class UpdateStockRequest
{
    [Range(0, int.MaxValue)] public int StockQuantity { get; set; }
}
