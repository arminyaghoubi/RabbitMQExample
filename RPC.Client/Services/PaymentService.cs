using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RPC.Shared.DTO;
using System.Text;
using System.Text.Json;

namespace RPC.Client.Services;

internal static class PaymentService
{
    public static async Task SaleTransaction(string? pan, decimal amount)
    {
        PaymentRequest request = new(Guid.NewGuid().ToString(), pan, amount);

        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var replyQueue = (await channel.QueueDeclareAsync()).QueueName;

        BasicProperties properties = new();
        properties.CorrelationId = request.PaymentId;
        properties.ReplyTo = replyQueue;

        var requestBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

        await channel.BasicPublishAsync("", "payment_rpc_sale", false, properties, requestBody);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            PaymentResponse response = JsonSerializer.Deserialize<PaymentResponse>(e.Body.ToArray());
            Console.WriteLine($"Response => {response.Message}");
        };
        await channel.BasicConsumeAsync(replyQueue, true, consumer);

        Console.ReadLine();
    }
}
