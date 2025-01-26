using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RPC.Shared.DTO;
using System.Text;
using System.Text.Json;

namespace RPC.Server.Services;

internal static class PaymentService
{
    public static async Task Sale()
    {
        ConnectionFactory factory = new();

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("payment_rpc_sale", false, false, false);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var replyProperties = new BasicProperties();
            replyProperties.CorrelationId = e.BasicProperties.CorrelationId;


            PaymentRequest request = JsonSerializer.Deserialize<PaymentRequest>(e.Body.ToArray());

            Console.WriteLine($"Received Request(PaymentId: {request.PaymentId})");

            PaymentResponse response;
            if (request.Pan.Length != 16)
                response = new(false, "Invalid PAN");
            else if (request.Amount < 1000)
                response = new(false, "Invalid Amount");
            else
                response = new(true, "Transaction successfully done.");

            string responseBody=JsonSerializer.Serialize(response);

            await channel.BasicPublishAsync("",e.BasicProperties.ReplyTo,true,replyProperties,Encoding.UTF8.GetBytes(responseBody));
            await channel.BasicAckAsync(e.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync("payment_rpc_sale", false, consumer);

        Console.ReadLine();
    }
}
