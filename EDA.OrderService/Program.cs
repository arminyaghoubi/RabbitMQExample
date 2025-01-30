using EDA.OrderService.Producers;

Console.WriteLine("Welcome to My Store");

Console.Write("Please enter your email: ");
string email = Console.ReadLine();

do
{
    OrderPlacedPublisher orderPlacedPublisher = new();

    Console.Write("Please select items[Product1, Product2, Product3]: ");
    var products = Console.ReadLine()?.Split(",");
    var orderId = Guid.NewGuid();

    await orderPlacedPublisher.PublishAsync(orderId, email, products);

    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.WriteLine("Order Placed Published.");
    Console.ResetColor();
} while (true);