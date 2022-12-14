using System.Net.Sockets;

namespace NetworkChat;

public class Client
{
    public async Task Start(int port, string host)
    {
        
        using (var client = new TcpClient(host, port))
        {
            var stream = client.GetStream();
            var listen = new Task(async () =>
            {
                var listener = new StreamReader(stream);
                while (true)
                {
                    string? line = await listener.ReadLineAsync();
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
            listen.Start();
            write.Start();
            await Task.WhenAny(listen, write);
        }
    }
}