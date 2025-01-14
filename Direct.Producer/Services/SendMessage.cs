using Direct.Shared.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace Direct.Producer.Services;

internal static class SendMessage
{
    public static async Task SendOrder(string? orderMessage, bool persistent = false)
    {
        OrderMessage order = new(
            Guid.NewGuid().ToString(),
            orderMessage,
            DateTime.Now
            );

        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "order",
            durable: true,
            exclusive: false,
            autoDelete: false);

        BasicProperties properties = new();
        if (persistent)
            properties.Persistent = true;

        var message = JsonSerializer.SerializeToUtf8Bytes(order);
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "order",
            mandatory: true,
            basicProperties: properties,
            body: message);
    }
}
