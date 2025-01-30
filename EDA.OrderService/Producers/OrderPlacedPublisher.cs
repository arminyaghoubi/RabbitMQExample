using EDA.Shared.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EDA.OrderService.Producers;

internal class OrderPlacedPublisher
{
    private readonly IChannel _channel;

    public OrderPlacedPublisher()
    {
        ConnectionFactory factory = new();
        var connection = factory.CreateConnectionAsync().Result;
        _channel = connection.CreateChannelAsync().Result;

        _channel.ExchangeDeclareAsync("order_exchange", ExchangeType.Topic).Wait();
    }

    public async Task PublishAsync(Guid orderId, string customerEmail, IEnumerable<string> items)
    {
        OrderPlacedEvent orderEvent = new(orderId, customerEmail, items);

        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderEvent));

        await _channel.BasicPublishAsync("order_exchange", "order.placed", message);
    }
}
