namespace Delayed.Shared.Messages;

public record InvoiceMessage(string BillId,string PayId,DateTime CreationDateTime);
