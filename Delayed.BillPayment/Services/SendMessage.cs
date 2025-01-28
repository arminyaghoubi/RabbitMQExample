using Delayed.Shared.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Delayed.BillPayment.Services;

internal static class SendMessage
{
    public static async Task CreateBillPayReminderAsync(string billId, string payId)
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("bill_reminder_exchange", ExchangeType.Direct, false, true);
        await channel.QueueDeclareAsync("bill_reminder", true, false, false);
        await channel.QueueBindAsync("bill_reminder", "bill_reminder_exchange", "bill_reminder");


        var properties = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange","bill_reminder_exchange"},
            { "x-dead-letter-routing-key","bill_reminder" },
            { "x-message-ttl",10000},// 10s
        };

        await channel.QueueDeclareAsync(
            "reminder_10s_queue",
            true,
            false,
            false,
            properties);

        InvoiceMessage invoice = new(billId, payId, DateTime.Now);

        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(invoice));

        await channel.BasicPublishAsync(
            "",
            "reminder_10s_queue",
            message);
    }
}
