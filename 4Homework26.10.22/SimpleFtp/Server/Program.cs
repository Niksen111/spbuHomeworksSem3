var source = new CancellationTokenSource();

var server = new Server.Server(source.Token);

Task.Run(() => server.Start());
if (Console.ReadKey().Key == ConsoleKey.Enter)
{
    source.Cancel();
}