using RPC.Client.Services;

await PaymentService.SaleTransaction("1234567898765432", 10000);

Console.ReadLine();