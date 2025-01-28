using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Priority.Consumer.Services;

internal static class ReceiveMessage
{
    public static async Task ReceiveAsync()
    {
        await Task.Delay(5000);

        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Dictionary<string, object?> properties = new Dictionary<string, object?>
        {
            {"x-max-priority",2 }
        };

        await channel.QueueDeclareAsync("priority-message-queue", true, false, false, properties);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.ForegroundColor = e.BasicProperties.Priority switch
            {
                1 => ConsoleColor.Blue,
                2 => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            Console.WriteLine(message);
        };

        await channel.BasicConsumeAsync("priority-message-queue", true, consumer);

        Console.ReadLine();
    }
}
