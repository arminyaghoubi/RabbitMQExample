using Headers.Producer.Services;

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Chat Sender");
Console.ResetColor();

do
{
    Console.Write("Please enter title message: ");
    string title = Console.ReadLine();
    Console.Write("Please enter description message: ");
    string description = Console.ReadLine();

    Console.Write("Please select a destination[admin, user]: ");
    string destination = Console.ReadLine();

    await SendMessage.SendAsync(title, description, destination);

    Console.WriteLine("Message Sent.");
} while (true);