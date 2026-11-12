using Moq;
using OrderProcessing.Core.Abstractions;
using OrderProcessing.Core.Models;
using OrderProcessing.Core.Services;
using Xunit;

namespace OrderProcessing.Tests.xUnit;

public sealed class OrderProcessingServiceTests
{
    private readonly Mock<IInventoryService> _inventory = new();
    private readonly Mock<IPaymentService> _payment = new();
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IOrderNotificationService> _notifications = new();

    private OrderProcessingService CreateSut() =>
        new(_inventory.Object, _payment.Object,
            _orders.Object, _notifications.Object);

    [Fact]
    public async Task PlaceOrderAsync_WhenAllStepsSucceed_ReturnsConfirmedOrder()
    {
        var request = new PlaceOrderRequest(
            "CUST-1",
            new[] { new OrderItem("BOOK", 2, 100m) },
            "payment-token");

        _inventory.Setup(x => x.ReserveAsync(
            "BOOK", 2, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _payment.Setup(x => x.ChargeAsync(
            "payment-token", 200m, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(true, "TX-100", null));

        var sut = CreateSut();

        var result = await sut.PlaceOrderAsync(request);

        Assert.Equal(200m, result.Total);
        Assert.Equal(OrderStatus.Confirmed, result.Status);
        _payment.Verify(x => x.ChargeAsync(
            "payment-token", 200m, result.OrderId,
            It.IsAny<CancellationToken>()), Times.Once);
        _orders.Verify(x => x.SaveConfirmedAsync(
            result.OrderId, "CUST-1",
            It.IsAny<IReadOnlyList<OrderItem>>(), 200m, "TX-100",
            It.IsAny<CancellationToken>()), Times.Once);
        _notifications.Verify(x => x.SendConfirmationAsync(
            result.OrderId, "CUST-1", 200m,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenInventoryUnavailable_DoesNotChargePayment()
    {
        var request = new PlaceOrderRequest(
            "CUST-2",
            new[] { new OrderItem("OUT-OF-STOCK", 1, 50m) },
            "token");

        _inventory.Setup(x => x.ReserveAsync(
            "OUT-OF-STOCK", 1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.PlaceOrderAsync(request));

        _payment.Verify(x => x.ChargeAsync(
            It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task PlaceOrderAsync_WhenPaymentDeclined_ReleasesReservation()
    {
        var request = new PlaceOrderRequest(
            "CUST-3", new[] { new OrderItem("BOOK", 2, 100m) }, "token");

        _inventory.Setup(x => x.ReserveAsync(
            "BOOK", 2, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _payment.Setup(x => x.ChargeAsync(
            "token", 200m, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(false, null, "Insufficient funds"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<PaymentDeclinedException>(
            () => sut.PlaceOrderAsync(request));

        _inventory.Verify(x => x.ReleaseAsync(
            "BOOK", 2, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _orders.Verify(x => x.SaveConfirmedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(),
            It.IsAny<IReadOnlyList<OrderItem>>(), It.IsAny<decimal>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifications.Verify(x => x.SendConfirmationAsync(
            It.IsAny<Guid>(), It.IsAny<string>(),
            It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenLaterItemCannotBeReserved_ReleasesEarlierReservation()
    {
        var request = new PlaceOrderRequest(
            "CUST-4",
            new[]
            {
            new OrderItem("P1", 1, 20m),
            new OrderItem("P2", 1, 30m)
            },
            "token");

        _inventory.Setup(x => x.ReserveAsync(
            "P1", 1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _inventory.Setup(x => x.ReserveAsync(
            "P2", 1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.PlaceOrderAsync(request));

        _inventory.Verify(x => x.ReleaseAsync(
            "P1", 1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _payment.Verify(x => x.ChargeAsync(
            It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task PlaceOrderAsync_WhenRequestIsInvalid_DoesNotCallDependencies()
    {
        var request = new PlaceOrderRequest(
            "", Array.Empty<OrderItem>(), "token");

        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.PlaceOrderAsync(request));

        _inventory.Verify(x => x.ReserveAsync(
            It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _payment.Verify(x => x.ChargeAsync(
            It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

}
