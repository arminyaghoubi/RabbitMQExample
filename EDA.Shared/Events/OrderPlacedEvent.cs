namespace EDA.Shared.Events;

public record OrderPlacedEvent(Guid OrderId,string CustomerEmail,IEnumerable<string> Items);
