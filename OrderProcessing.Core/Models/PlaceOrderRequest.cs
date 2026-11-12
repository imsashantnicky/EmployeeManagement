namespace OrderProcessing.Core.Models;

public sealed record PlaceOrderRequest(
    string CustomerId,
    IReadOnlyList<OrderItem> Items,
    string PaymentMethodToken);

