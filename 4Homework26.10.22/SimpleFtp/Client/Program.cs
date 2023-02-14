if (args.Length != 2)
{
    throw new InvalidDataException();
}

if (!int.TryParse(args[1], out int port) || port < 0 || port > 65535)
{
    throw new InvalidDataException();
}

var client = new Client.Client(args[0], port);

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

            var contents = await client.ListAsync(request[1]);
            var result = contents.Count + " ";
            foreach (var content in contents)
            {
                result += content.ToString();
            }

            Console.WriteLine(result);
            break;
        }

        case "2":
        {
            if (request.Length != 3)
            {
                throw new InvalidDataException();
            }

            var fileStream = File.OpenRead(request[2]);
            await client.GetAsync(request[1], fileStream);
            fileStream.Close();
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
        Console.WriteLine(" 2 <path> <path to the new file> download file from the server");
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