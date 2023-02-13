using System.Net.Sockets;

if (args.Length != 2)
{
    throw new InvalidDataException();
}

if (!int.TryParse(args[1], out int port) || port < 0 || port > 65535)
{
    throw new InvalidDataException();
}

using var tcpClient = new TcpClient();
await tcpClient.ConnectAsync(args[0], port);
await using var stream = tcpClient.GetStream();
var client = new Client.Client();

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
        PrintFunctions.PrintHelp();
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

            Console.WriteLine(await client.ListAsync(stream, request[1]));
            break;
        }

        case "2":
        {
            if (request.Length != 2)
            {
                throw new InvalidDataException();
            }

            var response = await client.GetAsync(stream, request[1]);
            Console.WriteLine(response);
            break;
        }

        case "3":
        {
            if (request.Length != 3)
            {
                throw new InvalidDataException();
            }

            await client.DownloadAsync(stream, request[1], request[2]);
            Console.WriteLine("Success.");
            break;
        }

        default:
        {
            PrintFunctions.PrintFailed();
            break;
        }
    }
}

/// <summary>
/// Functions for printing auxiliary information.
/// </summary>
static class PrintFunctions
{
    /// <summary>
    /// Prints a set of commands.
    /// </summary>
    public static void PrintHelp()
    {
        Console.WriteLine("Usage: [command] PATH");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine(" 1 <path>   listing of files in the directory on the server");
        Console.WriteLine(" 2 <path>   read file from the server");
        Console.WriteLine(" 3 <path> <path to the new file> download file from the server");
        Console.WriteLine();
        Console.WriteLine("q  -  quit");
    }

    /// <summary>
    /// Prints an error message.
    /// </summary>
    public static void PrintFailed()
    {
        Console.WriteLine("Failed to read request");
        Console.WriteLine("Type \"help\" to see a list of commands");
    }
}