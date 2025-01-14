using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Topic.Consumer.Services;

internal static class ErrorService
{
    public static async Task ReceiveErrorLog()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("topic_logs", ExchangeType.Topic);
        await channel.QueueDeclareAsync("logs_error",true,false,false);
        await channel.QueueBindAsync("logs_error", "topic_logs", "log.error");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR LOG: {message}");
        };

        await channel.BasicConsumeAsync("logs_error", true, consumer);

        Console.ReadKey();
    }
}