using EDA.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace EDA.InventoryService.Consumers;

internal static class OrderPlacedConsumer
{
    public static async Task ConsumeAsync()
    {
        ConnectionFactory factory = new();

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("inventory_queue", true, false, false);
        await channel.QueueBindAsync("inventory_queue", "order_exchange", "order.placed");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            OrderPlacedEvent orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(e.Body.ToArray());

            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in orderEvent.Items)
            {
                Console.WriteLine($"Inventory for {item} Updated.");
            }
            Console.ResetColor();
        };

        await channel.BasicConsumeAsync("inventory_queue", true, consumer);

        Console.WriteLine("Waiting for receive order.");
        Console.ReadLine();
    }
}
