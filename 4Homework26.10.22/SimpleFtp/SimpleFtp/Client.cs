using System.Net.Sockets;

namespace SimpleFtp;

public class Client
{
    public async Task Start(int port = 11111, string host = "localhost")
    {
        using (var client = new TcpClient(host, port))
        {
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
                    PrintHelp();
                    continue;
                }

                if (String.Compare(line, "q") == 0)
                {
                    return;
                }
                
                writer.WriteLine(line);
                writer.Flush();
                
                var data = reader.ReadToEnd();
                Console.WriteLine($"Received: \n{data}");
            }
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: [command] PATH");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine(" 1        listing of files in the directory on the server");
        Console.WriteLine(" 2        download file from the server");
        Console.WriteLine();
        Console.WriteLine("q  -  quite");
    }

    private static void PrintFailed()
    {
        Console.WriteLine("Failed to read request");
    }
}