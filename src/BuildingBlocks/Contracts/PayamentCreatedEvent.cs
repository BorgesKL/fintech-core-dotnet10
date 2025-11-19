namespace FintechCore.BuildingBlocks.Contracts;

// "record" é perfeito para eventos: imutável e leve.
public record PaymentCreatedEvent(
    Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateTime CreatedAt
);