public static class Program
{
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

    public static void Main(string[] args)
    {
        while (true)
        {

        }
    }
}