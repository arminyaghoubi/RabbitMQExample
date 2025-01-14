using Fanout.Shared.Messages;
using RabbitMQ.Client;
using System.Text.Json;

namespace Fanout.Producer.Services;

internal static class SendMessage
{
    public static async Task SendOrder(string? orderNumber, string? customerEmail, bool persistent = false)
    {
        OrderMessage orderMessage = new(Guid.NewGuid().ToString(), orderNumber, customerEmail, DateTime.Now);

        var factory = new ConnectionFactory { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "order_exchange",
            type: ExchangeType.Fanout);

        BasicProperties properties = new();
        if (persistent)
            properties.Persistent = true;

        var message = JsonSerializer.SerializeToUtf8Bytes(orderMessage);
        await channel.BasicPublishAsync(
            exchange: "order_exchange",
            routingKey: "",
            mandatory: true,
            basicProperties: properties,
            body: message);
    }
}
