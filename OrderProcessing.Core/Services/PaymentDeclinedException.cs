namespace OrderProcessing.Core.Services;

public sealed class PaymentDeclinedException : Exception
{
    public PaymentDeclinedException(string message) : base(message) { }
}

