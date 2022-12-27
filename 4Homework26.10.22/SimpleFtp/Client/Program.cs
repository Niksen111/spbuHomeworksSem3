namespace Client;

using System.Net.Sockets;

/// <summary>
/// Console application for interaction with the server.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Starts the application.
    /// </summary>
    /// <param name="args">Host Ip and port number.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            throw new InvalidDataException();
        }

        if (!int.TryParse(args[1], out int port) || port < 0 || port > 65535)
        {
            throw new InvalidDataException();
        }

        using var client = new TcpClient();
        await client.ConnectAsync(args[0], port);
        await using var stream = client.GetStream();
        var clinet = new Client();

        Console.WriteLine("Type \"help\" to see a list of commands");

        while (true)
        {
            var line = Console.ReadLine();
            if (line == null)
            {
                return;
            }

            if (string.CompareOrdinal(line, "q") == 0)
            {
                return;
            }

            if (string.CompareOrdinal(line, "help") == 0)
            {
                PrintHelp();
                continue;
            }

            var request = line.Split();

            switch (request[0])
            {
                case "1":
                {
                    if (request.Length != 2)
                    {
                        throw new InvalidDataException();
                    }

                    Console.WriteLine(await clinet.ListAsync(stream, request[1]));
                    break;
                }

                case "2":
                {
                    if (request.Length != 3)
                    {
                        throw new InvalidDataException();
                    }

                    await clinet.GetAsync(stream, request[1], request[2]);
                    break;
                }

                default:
                {
                    PrintFailed();
                    break;
                }
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