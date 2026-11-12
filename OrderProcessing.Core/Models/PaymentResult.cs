namespace OrderProcessing.Core.Models;

public sealed record PaymentResult(
    bool Succeeded,
    string? TransactionId,
    string? FailureReason);

