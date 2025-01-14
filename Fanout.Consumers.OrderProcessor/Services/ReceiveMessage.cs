using Fanout.Shared.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Fanout.Consumers.OrderProcessor.Services;

internal static class ReceiveMessage
{
    public static async Task ReceiveOrder()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "orderprocessor_order",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: "orderprocessor_order",
            exchange: "order_exchange",
            routingKey: "");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var message = JsonSerializer.Deserialize<OrderMessage>(e.Body.ToArray());

            Console.WriteLine($"Has been order: {message.OrderNumber} processed.");

            await channel.BasicAckAsync(
                deliveryTag: e.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "orderprocessor_order",
            autoAck: false,
            consumer: consumer);

        Console.WriteLine("Waiting for orders...");
        Console.ReadKey();
    }
}
