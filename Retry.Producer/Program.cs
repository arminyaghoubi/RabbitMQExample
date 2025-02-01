using Retry.Producer.Services;

do
{
    Console.Write("Please enter your email: ");
    string email = Console.ReadLine();

    await PublisherService.PublishNotificationAsync(email);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Notification Sent to Notification Service.");
    Console.ResetColor();

} while (true);