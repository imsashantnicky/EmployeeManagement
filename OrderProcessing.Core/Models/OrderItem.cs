namespace OrderProcessing.Core.Models;

public sealed record OrderItem(
    string ProductId,
    int Quantity,
    decimal UnitPrice);


