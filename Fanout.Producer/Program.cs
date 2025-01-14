using Fanout.Producer.Services;

do
{
    Console.Write("Please enter order message: ");
    string? orderMessage = Console.ReadLine();

    Console.Write("Please enter customer email: ");
    string? customerEmail = Console.ReadLine();

    await SendMessage.SendOrder(orderMessage, customerEmail, true);

    Console.WriteLine("Order Sent to OrderProcessor, OrderSaver and EmailSender Services.");
} while (true);