using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Retry.Consumer.Services;

internal static class ConsumerService
{
    public static async Task ConsumeAsync()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("email_exchange", ExchangeType.Direct, true, false);

        var emailProperties = new Dictionary<string, object?>
        {
            {"x-dead-letter-exchange","email_dlx" },
            {"x-dead-letter-routing-key","email_dead_queue" }
        };

        await channel.QueueDeclareAsync("email_queue", true, false, false, emailProperties);
        await channel.QueueBindAsync("email_queue", "email_exchange", "email_queue");

        await channel.ExchangeDeclareAsync("email_dlx", ExchangeType.Direct, true, false);
        await channel.QueueDeclareAsync("email_dead_queue", true, false, false);
        await channel.QueueBindAsync("email_dead_queue", "email_dlx", "email_dead_queue");

        var delayedProperties = new Dictionary<string, object?>
        {
            {"x-dead-letter-exchange","email_exchange" },
            {"x-dead-letter-routing-key","email_queue" },
            {"x-message-ttl",5000 }
        };

        await channel.QueueDeclareAsync("email_retry_queue", true, false, false, delayedProperties);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[X] Received {message}");
            Console.ResetColor();

            int retryCount = 0;
            if (e.BasicProperties.Headers != null && e.BasicProperties.Headers.ContainsKey("x-retry-count"))
                retryCount = (int)e.BasicProperties.Headers["x-retry-count"];

            if (retryCount < 3)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}] Processing failed. Retrying ({++retryCount})...");
                Console.ResetColor();

                BasicProperties retryProperties = new();
                retryProperties.Headers = new Dictionary<string, object?>
                {
                    { "x-retry-count", retryCount }
                };

                await channel.BasicPublishAsync("", "email_retry_queue", true, retryProperties, e.Body);

                await channel.BasicAckAsync(e.DeliveryTag, false);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[X] Message failed after 3 retries. Sending to DLQ.");
                Console.ResetColor();

                await channel.BasicRejectAsync(e.DeliveryTag, false);
            }
        };

        await channel.BasicConsumeAsync("email_queue", false, consumer);

        Console.WriteLine("Waiting for message...");
        Console.ReadLine();
    }
}
