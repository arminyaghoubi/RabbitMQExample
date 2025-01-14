using Fanout.Shared.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Fanout.Consumers.EmailSender.Services;

internal static class ReceiveMssage
{
    public static async Task ReceiveOrder()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "emailsender_order",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: "emailsender_order",
            exchange: "order_exchange",
            routingKey: "");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var message = JsonSerializer.Deserialize<OrderMessage>(e.Body.ToArray());

            Console.WriteLine($"Email sent to {message.CustomerEmail}.");

            await channel.BasicAckAsync(
                deliveryTag: e.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "emailsender_order",
            autoAck: false,
            consumer: consumer);

        Console.WriteLine("Waiting for orders...");
        Console.ReadKey();
    }
}
