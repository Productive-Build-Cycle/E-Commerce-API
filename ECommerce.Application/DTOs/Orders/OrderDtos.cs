using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders;

public sealed record OrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record OrderResponse(
    int Id,
    int UserId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] OrderStatus Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal FinalTotal,
    IReadOnlyList<OrderItemResponse> Items);

public sealed class CreateOrderRequest
{
    [Required, MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = [];

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal DiscountAmount { get; set; }
}

public sealed class CreateOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public sealed class UpdateOrderStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus Status { get; set; }
}
