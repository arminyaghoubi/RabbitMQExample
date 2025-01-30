using EDA.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace EDA.EmailService.Consumers;

internal static class PaymentCompletedConsumer
{
    public static async Task ConsumeAsync()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("email_queue", true, false, false);
        await channel.QueueBindAsync("email_queue", "order_exchange", "payment.completed");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            PaymentCompletedEvent paymentEvent = JsonSerializer.Deserialize<PaymentCompletedEvent>(e.Body.ToArray());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Email Sent for {paymentEvent.Email}");
            Console.ResetColor();
            Console.WriteLine("--------------------------------------");
        };

        await channel.BasicConsumeAsync("email_queue", true, consumer);

        Console.WriteLine("Waiting for Payment Completed.");
        Console.ReadLine();
    }
}
