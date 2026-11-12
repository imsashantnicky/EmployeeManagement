using Moq;
using NUnit.Framework;
using OrderProcessing.Core.Abstractions;
using OrderProcessing.Core.Models;
using OrderProcessing.Core.Services;

namespace OrderProcessing.Tests.NUnit;

[TestFixture]
public sealed class OrderProcessingServiceTests
{
    private Mock<IInventoryService> _inventory = null!;
    private Mock<IPaymentService> _payment = null!;
    private Mock<IOrderRepository> _orders = null!;
    private Mock<IOrderNotificationService> _notifications = null!;

    [SetUp]
    public void SetUp()
    {
        _inventory = new Mock<IInventoryService>();
        _payment = new Mock<IPaymentService>();
        _orders = new Mock<IOrderRepository>();
        _notifications = new Mock<IOrderNotificationService>();
    }

    private OrderProcessingService CreateSut() =>
        new(_inventory.Object, _payment.Object,
            _orders.Object, _notifications.Object);

    [Test]
    public async Task PlaceOrderAsync_WhenSuccessful_ReturnsConfirmed()
    {
        var request = new PlaceOrderRequest(
            "CUST-1", new[] { new OrderItem("BOOK", 2, 100m) }, "token");

        _inventory.Setup(x => x.ReserveAsync(
            "BOOK", 2, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _payment.Setup(x => x.ChargeAsync(
            "token", 200m, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(true, "TX-1", null));

        var result = await CreateSut().PlaceOrderAsync(request);

        Assert.That(result.Total, Is.EqualTo(200m));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.Confirmed));
        _notifications.Verify(x => x.SendConfirmationAsync(
            result.OrderId, "CUST-1", 200m,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void PlaceOrderAsync_WhenInventoryUnavailable_Throws()
    {
        var request = new PlaceOrderRequest(
            "CUST-2", new[] { new OrderItem("P1", 1, 20m) }, "token");

        _inventory.Setup(x => x.ReserveAsync(
            "P1", 1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await CreateSut().PlaceOrderAsync(request));

        _payment.Verify(x => x.ChargeAsync(
            It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void PlaceOrderAsync_WhenPaymentDeclined_ThrowsPaymentDeclined()
    {
        var request = new PlaceOrderRequest(
            "CUST-3", new[] { new OrderItem("P1", 1, 20m) }, "token");

        _inventory.Setup(x => x.ReserveAsync(
            "P1", 1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _payment.Setup(x => x.ChargeAsync(
            "token", 20m, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(false, null, "Declined"));

        Assert.ThrowsAsync<PaymentDeclinedException>(
            async () => await CreateSut().PlaceOrderAsync(request));

        _inventory.Verify(x => x.ReleaseAsync(
            "P1", 1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
