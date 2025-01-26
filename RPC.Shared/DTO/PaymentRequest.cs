namespace RPC.Shared.DTO;

public record PaymentRequest(string? PaymentId, string? Pan, decimal Amount);