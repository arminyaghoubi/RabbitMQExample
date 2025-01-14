using RabbitMQ.Client;
using System.Text;

namespace DLQ.Producer.Services;

internal static class SendMessage
{
    public static async Task SendOrder(string? message, bool persistent = false)
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var arguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange","dlx"},
        };

        await channel.ExchangeDeclareAsync("dlx", ExchangeType.Fanout);
        await channel.QueueDeclareAsync(
            queue: "orders",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments);

        BasicProperties properties = new();
        if (persistent)
            properties.Persistent = true;

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "orders",
            mandatory: true,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(message));

        Console.WriteLine("Message sent.");
    }
}
