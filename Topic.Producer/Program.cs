using Topic.Producer.Services;

do
{
    Console.Write("Please enter log level[info, warning, error]: ");
    string? level = Console.ReadLine();

    Console.Write("Please enter log message: ");
    string? message = Console.ReadLine();

    await SendMessage.SendLog(message,level);
} while (true);