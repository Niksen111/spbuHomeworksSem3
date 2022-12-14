using System.Net;
using System.Net.Sockets;

namespace NetworkChat;

public class Server
{
    public async Task Start(int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Listening on port {port}...");
        
        using (var socket = await listener.AcceptSocketAsync())
        {
            var stream = new NetworkStream(socket);
            var listen = new Task(async () =>
            {
                var listener1 = new StreamReader(stream);
                while (true)
                {
                    var line = await listener1.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }
                    Console.WriteLine(line);
                    if (String.Compare(line, "exit") == 0)
                    {
                        break;
                    }
                }
            });
            
            var write = new Task(async () =>
            {
                var writer = new StreamWriter(stream);
                while (true)
                {
                    string line = Console.ReadLine()!;
                    await writer.WriteLineAsync(line);
                    await writer.FlushAsync();
                    if (String.Compare(line, "exit") == 0)
                    {
                        break;
                    }
                }
            });
            await Task.WhenAny(listen, write);
        }
        listener.Stop();
        
    }
}