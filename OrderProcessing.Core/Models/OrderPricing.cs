using OrderProcessing.Core.Models;

namespace OrderProcessing.Core;

public static class OrderPricing
{
    public static decimal CalculateTotal(IReadOnlyList<OrderItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
            throw new ArgumentException("An order must contain at least one item.", nameof(items));

        decimal total = 0m;

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.ProductId))
                throw new ArgumentException("Product ID is required.", nameof(items));

            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(items));

            if (item.UnitPrice <= 0m)
                throw new ArgumentException("Unit price must be greater than zero.", nameof(items));

            total += item.Quantity * item.UnitPrice;
        }

        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
