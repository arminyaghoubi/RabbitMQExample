using EDA.PaymentService.Publishers;
using EDA.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace EDA.PaymentService.Consumers;

internal static class OrderPlacedConsumer
{
    public static async Task ConsumeAsync()
    {
        ConnectionFactory factory = new();

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("payment_queue", true, false, false);
        await channel.QueueBindAsync("payment_queue", "order_exchange", "order.placed");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            OrderPlacedEvent orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(e.Body.ToArray());

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Payment for the order has been completed successfully.");
            Console.ResetColor();

            PaymentCompletedPublisher paymentCompletedPublisher = new();
            await paymentCompletedPublisher.PublishAsync(orderEvent.OrderId, orderEvent.CustomerEmail);

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Payment Completed Event Published.");
            Console.ResetColor();
        };

        await channel.BasicConsumeAsync("payment_queue", true, consumer);

        Console.WriteLine("Waiting for receive order.");
        Console.ReadLine();
    }
}