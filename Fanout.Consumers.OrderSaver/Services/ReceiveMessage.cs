using Fanout.Shared.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Fanout.Consumers.OrderSaver.Services;

internal static class ReceiveMessage
{
    public static async Task ReceiveOrder()
    {
        ConnectionFactory factory = new() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "ordersaver_order",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: "ordersaver_order",
            exchange: "order_exchange",
            routingKey: "");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var message = JsonSerializer.Deserialize<OrderMessage>(e.Body.ToArray());

            Console.WriteLine($"New Order: {message.OrderNumber}, {message.CustomerEmail} Saved.");

            await channel.BasicAckAsync(
                deliveryTag: e.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "ordersaver_order",
            autoAck: false,
            consumer);

        Console.WriteLine("Waiting for orders...");
        Console.ReadKey();
    }
}
