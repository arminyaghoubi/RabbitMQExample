using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Topic.Consumer.Services;

internal static class InfoService
{
    public static async Task ReceiveInfoLog()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("topic_logs", ExchangeType.Topic);
        await channel.QueueDeclareAsync("logs_info", true, false, false);
        await channel.QueueBindAsync("logs_info", "topic_logs", "log.info");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.ForegroundColor= ConsoleColor.Blue;
            Console.WriteLine($"INFO LOG: {message}");
        };

        await channel.BasicConsumeAsync("logs_info", true, consumer);

        Console.ReadKey();
    }
}