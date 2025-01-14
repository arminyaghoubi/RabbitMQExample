using RabbitMQ.Client;
using System.Text;

namespace Topic.Producer.Services;

internal static class SendMessage
{
    public static async Task SendLog(string? message, string? level)
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("topic_logs", ExchangeType.Topic);

        string routingKey = level switch
        {
            "info" => "log.info",
            "warning" => "log.warning",
            "error" => "log.error",
            _ => throw new ArgumentException()
        };

        await channel.BasicPublishAsync(
            exchange: "topic_logs",
            routingKey: routingKey,
            mandatory: true,
            body: Encoding.UTF8.GetBytes(message));

        Console.WriteLine($"Log Sent.");
    }
}
