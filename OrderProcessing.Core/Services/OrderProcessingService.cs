using OrderProcessing.Core.Abstractions;
using OrderProcessing.Core.Models;

namespace OrderProcessing.Core.Services;

public sealed class OrderProcessingService
{
    private readonly IInventoryService _inventory;
    private readonly IPaymentService _payment;
    private readonly IOrderRepository _orders;
    private readonly IOrderNotificationService _notifications;

    public OrderProcessingService(
        IInventoryService inventory,
        IPaymentService payment,
        IOrderRepository orders,
        IOrderNotificationService notifications)
    {
        _inventory = inventory;
        _payment = payment;
        _orders = orders;
        _notifications = notifications;
    }

    public async Task<OrderResult> PlaceOrderAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var total = OrderPricing.CalculateTotal(request.Items);
        var orderId = Guid.NewGuid();
        var reservedItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var reserved = await _inventory.ReserveAsync(
                item.ProductId, item.Quantity, orderId, cancellationToken);

            if (!reserved)
            {
                await ReleaseReservationsAsync(
                    reservedItems, orderId, cancellationToken);

                throw new InvalidOperationException(
                    $"Insufficient inventory for product {item.ProductId}.");
            }

            reservedItems.Add(item);
        }

        PaymentResult paymentResult;
        try
        {
            paymentResult = await _payment.ChargeAsync(
                request.PaymentMethodToken, total, orderId, cancellationToken);
        }
        catch
        {
            await ReleaseReservationsAsync(
                reservedItems, orderId, cancellationToken);
            throw;
        }

        if (!paymentResult.Succeeded)
        {
            await ReleaseReservationsAsync(
                reservedItems, orderId, cancellationToken);

            throw new PaymentDeclinedException(
                paymentResult.FailureReason ?? "Payment was declined.");
        }

        try
        {
            await _orders.SaveConfirmedAsync(
                orderId,
                request.CustomerId,
                request.Items,
                total,
                paymentResult.TransactionId!,
                cancellationToken);
        }
        catch
        {
            // The payment may already have been charged.
            // A real system needs a refund/void or reconciliation workflow.
            throw;
        }

        await _notifications.SendConfirmationAsync(
            orderId, request.CustomerId, total, cancellationToken);

        return new OrderResult(orderId, total, OrderStatus.Confirmed);
    }

    private async Task ReleaseReservationsAsync(
        IEnumerable<OrderItem> items,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        foreach (var item in items.Reverse())
        {
            await _inventory.ReleaseAsync(
                item.ProductId, item.Quantity, orderId, cancellationToken);
        }
    }

    private static void ValidateRequest(PlaceOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CustomerId))
            throw new ArgumentException("Customer ID is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.PaymentMethodToken))
            throw new ArgumentException("Payment method token is required.", nameof(request));

        _ = OrderPricing.CalculateTotal(request.Items);
    }
}
