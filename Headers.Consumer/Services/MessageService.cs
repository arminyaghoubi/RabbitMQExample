using Headers.Shared.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Headers.Consumer.Services;

internal static class MessageService
{
    public static async Task ReceiveMessage()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("chat_header_exchange", ExchangeType.Headers, false, true);

        #region Admin
        Dictionary<string, object?> adminProperties = new Dictionary<string, object?>
        {
            {"x-match","all" },
            {"user-type","admin" },
        };
        await channel.QueueDeclareAsync("chat_admin_queue", true, false, false);
        await channel.QueueBindAsync("chat_admin_queue", "chat_header_exchange", "", adminProperties);

        AsyncEventingBasicConsumer adminConsumer = new(channel);
        adminConsumer.ReceivedAsync += async (sender, e) =>
        {
            ChatMessage message = JsonSerializer.Deserialize<ChatMessage>(Encoding.UTF8.GetString(e.Body.ToArray()));

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Admin Message:\n\t{message.Title}\n\t{message.Description}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------");
        };
        await channel.BasicConsumeAsync("chat_admin_queue", true, adminConsumer);
        #endregion

        #region User
        Dictionary<string, object?> userProperties = new Dictionary<string, object?>
        {
            {"x-match","all" },
            {"user-type","user" },
        };
        await channel.QueueDeclareAsync("chat_user_queue", true, false, false);
        await channel.QueueBindAsync("chat_user_queue", "chat_header_exchange", "", userProperties);

        AsyncEventingBasicConsumer userConsumer = new(channel);
        userConsumer.ReceivedAsync += async (sender, e) =>
        {
            ChatMessage message = JsonSerializer.Deserialize<ChatMessage>(Encoding.UTF8.GetString(e.Body.ToArray()));

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"User Message:\n\t{message.Title}\n\t{message.Description}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------");
        };
        await channel.BasicConsumeAsync("chat_user_queue", true, userConsumer);
        #endregion

        Console.ReadLine();
    }
}
