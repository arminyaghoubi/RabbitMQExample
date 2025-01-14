namespace Fanout.Shared.Messages;

public record OrderMessage(string? Id, string? OrderNumber, string? CustomerEmail, DateTime CreationDate);
