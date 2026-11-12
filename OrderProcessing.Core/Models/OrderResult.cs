namespace OrderProcessing.Core.Models;

public enum OrderStatus
{
    Confirmed
}

public sealed record OrderResult(
    Guid OrderId,
    decimal Total,
    OrderStatus Status);
