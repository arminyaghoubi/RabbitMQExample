using EDA.Shared.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EDA.PaymentService.Publishers;

internal class PaymentCompletedPublisher
{
    public async Task PublishAsync(Guid orderId,string email)
    {
        ConnectionFactory factory = new();
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        PaymentCompletedEvent paymentEvent=new(orderId,email);
        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(paymentEvent));

        await channel.BasicPublishAsync("order_exchange", "payment.completed", message);
    }
}
