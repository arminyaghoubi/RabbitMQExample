using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DLQ.Consumer.Services;

internal static class ReceiveMessage
{
    public static async Task ReceiveOrder()
    {
        ConnectionFactory factory = new() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var arguments = new Dictionary<string, object>
        {
            {"x-dead-letter-exchange","dlx" }
        };

        await channel.QueueDeclareAsync(
            queue: "orders",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments);

        await channel.ExchangeDeclareAsync("dlx", ExchangeType.Fanout);
        await channel.QueueDeclareAsync("dead_order", true, false, false);
        await channel.QueueBindAsync("dead_order","dlx","");

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());

            try
            {
                if (message == "Error")
                {
                    throw new Exception("Error processing order.");
                }

                Console.WriteLine($"{message} received.");
                await channel.BasicAckAsync(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await channel.BasicNackAsync(e.DeliveryTag, false, false);
            }
        };

        await channel.BasicConsumeAsync("orders", false, consumer);

        Console.ReadKey();
    }
}
