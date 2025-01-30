namespace EDA.Shared.Events;

public record PaymentCompletedEvent(Guid OrderId,string Email);
