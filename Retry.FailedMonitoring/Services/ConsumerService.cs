using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Retry.FailedMonitoring.Services;

internal static class ConsumerService
{
    public static async Task ConsumeDeadEmailAsync()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("email_dlx", ExchangeType.Direct, true, false);
        await channel.QueueDeclareAsync("email_dead_queue", true, false, false);
        await channel.QueueBindAsync("email_dead_queue", "email_dlx", "email_dead_queue");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"({DateTime.Now}) Dead Letter Message: {message}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------");
        };

        await channel.BasicConsumeAsync("email_dead_queue", true, consumer);

        Console.WriteLine("Monitoring...");
        Console.ReadLine();
    }
}
