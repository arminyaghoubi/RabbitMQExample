using RabbitMQ.Client;
using System.Text;

namespace Priority.Producer.Services;

internal static class SendMessage
{
    public static async Task SendAsync(string message, byte priority = 0)
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Dictionary<string, object?> properties = new Dictionary<string, object?>
        {
            {"x-max-priority",2 }
        };

        await channel.QueueDeclareAsync("priority-message-queue", true, false, false, properties);

        BasicProperties messageProperties = new();
        messageProperties.Priority = priority;

        await channel.BasicPublishAsync(
            "",
            "priority-message-queue",
            true,
            messageProperties,
            Encoding.UTF8.GetBytes(message));

        Console.WriteLine($"{message} Sent.");
    }
}
