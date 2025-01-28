using Priority.Producer.Services;

await Parallel.ForAsync(0, 10, async (index, cancellation) =>
{
    Random random = new(index);
    byte priority = (byte)random.Next(0, 3);
    string message = $"Priority Message {priority}";
    await SendMessage.SendAsync(message, priority);
});