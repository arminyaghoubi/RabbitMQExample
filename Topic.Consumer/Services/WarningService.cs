using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Topic.Consumer.Services;

internal static class WarningService
{
    public static async Task ReceiveWarningLog()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("topic_logs", ExchangeType.Topic);
        await channel.QueueDeclareAsync("logs_warning", true, false, false);
        await channel.QueueBindAsync("logs_warning", "topic_logs", "log.warning");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARNING LOG: {message}");
        };

        await channel.BasicConsumeAsync("logs_warning", true, consumer);

        Console.ReadKey();
    }
}
