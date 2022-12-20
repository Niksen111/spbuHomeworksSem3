using System.Net;
using System.Net.Sockets;

namespace Server;

/// <summary>
/// A server handling two requests:
/// List - listing files in the directory on the server
/// Get - downloading a file from the server
/// </summary>
public class Server
{
    public async Task Start(int port = 11111)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Listening on port {port}...");

        using (var socket = await listener.AcceptSocketAsync())
        {
            var stream = new NetworkStream(socket);
            var listen = new StreamReader(stream);
            var write = new StreamWriter(stream);
            while (true)
            {
                var line = await listen.ReadLineAsync();
                if (line == null)
                {
                    break;
                }
                
            }
        }
    }
    
    
}