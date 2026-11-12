using OrderProcessing.Core;
using OrderProcessing.Core.Models;
using Xunit;

namespace OrderProcessing.Tests.xUnit;

public sealed class OrderPricingTests
{
    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(2, 10.50, 21.00)]
    [InlineData(3, 99.99, 299.97)]
    public void CalculateTotal_ForOneLine_ReturnsQuantityTimesPrice(
    int quantity, decimal unitPrice, decimal expected)
    {
        var items = new[] { new OrderItem("SKU-1", quantity, unitPrice) };

        Assert.Equal(expected, OrderPricing.CalculateTotal(items));
    }

    [Fact]
    public void CalculateTotal_WhenOrderHasNoItems_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => OrderPricing.CalculateTotal(Array.Empty<OrderItem>()));
    }


}
