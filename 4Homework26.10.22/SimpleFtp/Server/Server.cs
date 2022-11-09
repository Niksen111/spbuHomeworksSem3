using System.Net;
using System.Net.Sockets;

namespace Server;

public class Server
{
    public static async Task Main(string[] args)
    {
        const int port = 11111;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        
        while (true)
        {
            var socket = await listener.AcceptSocketAsync();
            Task.Run(async () =>
            {
                using (socket)
                {
                    var stream = new NetworkStream(socket);
                    var reader = new StreamReader(stream);
                    var data = await reader.ReadLineAsync();
                    if (data == null)
                    {
                        return;
                    }

                    var request = data.Split();
                    if (request.Length != 2)
                    {
                        
                    }
                }
            });
        }
    }
}