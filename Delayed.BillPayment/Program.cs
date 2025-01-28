using Delayed.BillPayment.Services;

Console.ForegroundColor = ConsoleColor.DarkGreen;
Console.WriteLine("Bill Reminder");
Console.ResetColor();

do
{
    Console.Write("Please enter a bill id: ");
    string billId = Console.ReadLine();
    Console.Write("Please enter a payment id: ");
    string payId = Console.ReadLine();

    await SendMessage.CreateBillPayReminderAsync(billId, payId);

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Billing alarm will be done in 10 seconds.");
    Console.ResetColor();
} while (true);