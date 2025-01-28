using Headers.Shared.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Headers.Producer.Services;

internal static class SendMessage
{
    public static async Task SendAsync(string title, string description, string destination = "admin")
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("chat_header_exchange", ExchangeType.Headers, false, true);

        Dictionary<string, object?> adminProperties = new Dictionary<string, object?>
        {
            {"x-match","all" },
            {"user-type","admin" },
        };
        await channel.QueueDeclareAsync("chat_admin_queue", true, false, false);
        await channel.QueueBindAsync("chat_admin_queue", "chat_header_exchange", "", adminProperties);

        Dictionary<string, object?> userProperties = new Dictionary<string, object?>
        {
            {"x-match","all" },
            {"user-type","user" },
        };
        await channel.QueueDeclareAsync("chat_user_queue", true, false, false);
        await channel.QueueBindAsync("chat_user_queue", "chat_header_exchange", "", userProperties);

        ChatMessage chatMessage = new(title, description);
        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(chatMessage));
        BasicProperties messageProperties = new();
        messageProperties.Headers = new Dictionary<string, object?>
        {
            { "user-type",destination}
        };

        await channel.BasicPublishAsync(
            "chat_header_exchange",
            "",
            true,
            messageProperties,
            message);
    }
}
