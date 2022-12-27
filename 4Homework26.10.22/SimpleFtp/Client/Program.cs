public static class Program
{
    private static void PrintHelp()
    {
        var client = new Client.Client();

        client.Start().Wait();
    }

    public static void Main(string[] args)
    {
        
    }
}