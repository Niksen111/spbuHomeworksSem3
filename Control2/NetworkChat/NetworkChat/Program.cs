namespace NetworkChat;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            args = new string[] {"localhost", "8080"};

        }
        if (args.Length == 1)
        {
            var server = new Server();
            await server.Start(int.Parse(args[0]));
        }
        else if (args.Length == 2)
        {
            var client = new Client();
            await client.Start(int.Parse(args[1]), args[0]);
        }
        else
        {
            Console.WriteLine("Invalid arguments");
        }
    }
}
