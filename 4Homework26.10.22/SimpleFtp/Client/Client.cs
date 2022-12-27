using System.Net.Sockets;

namespace Client;

/// <summary>
/// Client allowing to execute requests:
/// List - listing files in the directory on the server
/// Get - downloading a file from the server
/// </summary>
public class Client
{
    /// <summary>
    /// Starts client
    /// </summary>
    /// <param name="port">The port number of the remote host to which you intend to connect.</param>
    /// <param name="host">The DNS name of the remote host to which you intend to connect.</param>
    public async Task Start(int port = 11111, string host = "localhost")
    {
        using var client = new TcpClient(host, port);
        var stream = client.GetStream();
        Console.WriteLine("Print  help  to get help");
        var writer = new StreamWriter(stream);
        var reader = new StreamReader(stream);
        while (true)
        {
            var line = Console.ReadLine();
            if (line == null)
            {
                PrintFailed();
                continue;
            }

            if (String.Compare(line, "help") == 0)
            {
                continue;
            }

            if (String.Compare(line, "q") == 0)
            {
                return;
            }

            await writer.WriteLineAsync(line);
            writer.Flush();

            var data = await reader.ReadToEndAsync();
            Console.WriteLine($"Received: \n{data}");
        }
    }

    private static void PrintFailed()
    {
        Console.WriteLine("Failed to read request");
    }
}