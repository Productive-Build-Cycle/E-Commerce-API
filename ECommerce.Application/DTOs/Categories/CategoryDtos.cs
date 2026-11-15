using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Categories;

public sealed record CategoryResponse(int Id, string Name, bool IsActive);

public class CreateCategoryRequest
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateCategoryRequest : CreateCategoryRequest;
