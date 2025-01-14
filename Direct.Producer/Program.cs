using Direct.Producer.Services;

do
{
    Console.Write("Please enter order message: ");
    string? orderMessage = Console.ReadLine();

    await SendMessage.SendOrder(orderMessage, true);

    Console.WriteLine("Order Sent.");
} while (true);