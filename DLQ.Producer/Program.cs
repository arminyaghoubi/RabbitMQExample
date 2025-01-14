using DLQ.Producer.Services;

do
{
    Console.Write("Please enter order message: ");
    string? message = Console.ReadLine();
    await SendMessage.SendOrder(message, true);
} while (true);