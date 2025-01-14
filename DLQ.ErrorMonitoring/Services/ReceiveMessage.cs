using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DLQ.ErrorMonitoring.Services;

internal static class ReceiveMessage
{
    public static async Task ReceiveError()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "dead_order",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.ExchangeDeclareAsync(
            exchange: "dlx",
            type: ExchangeType.Fanout);

        await channel.QueueBindAsync(
            queue: "dead_order",
            exchange: "dlx",
            routingKey: "");

        Console.ForegroundColor = ConsoleColor.Red;
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.WriteLine(message);

            await channel.BasicAckAsync(
                deliveryTag: e.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync("dead_order", false, consumer);

        Console.ReadKey();
    }
}
