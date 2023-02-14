if (!int.TryParse(args[0], out int port) || port < 0 || port > 65535)
{
    throw new InvalidDataException();
}

var source = new CancellationTokenSource();

var server = new Server.Server();

var serverState = Task.Run(() => server.Start(source.Token, port));
Console.WriteLine("Press Enter to stop the server.");
while (true)
{
    if (Console.ReadKey().Key == ConsoleKey.Enter)
    {
        source.Cancel();
        await serverState;
        break;
    }
}