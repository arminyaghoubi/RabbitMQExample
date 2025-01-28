using Delayed.Shared.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Delayed.Notification.Services;

internal static class BillNotificationService
{
    public static async Task Notify()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("bill_reminder_exchange", ExchangeType.Direct, false, true);
        await channel.QueueDeclareAsync("bill_reminder", true, false, false);
        await channel.QueueBindAsync("bill_reminder", "bill_reminder_exchange", "bill_reminder");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"Received Message({DateTime.Now})");
                var message = JsonSerializer.Deserialize<InvoiceMessage>(Encoding.UTF8.GetString(e.Body.ToArray()));

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"========Bill Reminder\nBill ID: {message.BillId}\nPay ID: {message.PayId}\nCreation Date Time: {message.CreationDateTime}");
                Console.ResetColor();
            });
        };

        await channel.BasicConsumeAsync("bill_reminder", true, consumer);

        Console.WriteLine("Waiting for bill notifications.");
        Console.ReadLine();
    }
}
