if (!int.TryParse(args[0], out int port) || port < 1 || port > 65535)
{
    throw new InvalidDataException();
}

var source = new CancellationTokenSource();

var server = new Server.Server(source.Token);

Task.Run(() => server.Start(port));
if (Console.ReadKey().Key == ConsoleKey.Enter)
{
    source.Cancel();
}