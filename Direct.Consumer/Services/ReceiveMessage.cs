using Direct.Shared.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Direct.Consumer.Services;

internal static class ReceiveMessage
{
    public static async Task ReceiveOrder()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "order",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var order = JsonSerializer.Deserialize<OrderMessage>(e.Body.ToArray());

            Console.WriteLine($"Order Received: {order.Id}|{order.Message}|{order.CreationDate}");

            await channel.BasicAckAsync(
                deliveryTag: e.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "order",
            autoAck: false,
            consumer: consumer);

        Console.WriteLine("Waiting for orders...");
        Console.ReadKey();
    }
}
