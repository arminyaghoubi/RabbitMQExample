using RabbitMQ.Client;
using System.Text;

namespace Retry.Producer.Services;

internal static class PublisherService
{
    public static async Task PublishNotificationAsync(string email)
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var properties = new Dictionary<string, object?>
        {
            {"x-dead-letter-exchange","email_dlx" },
            {"x-dead-letter-routing-key","email_dead_queue" }
        };

        await channel.QueueDeclareAsync("email_queue", true, false, false, properties);

        var message=Encoding.UTF8.GetBytes(email);

        await channel.BasicPublishAsync("", "email_queue", message);
    }
}
