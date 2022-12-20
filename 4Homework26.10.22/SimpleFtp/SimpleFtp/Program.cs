using SimpleFtp;

if (args.Length == 1)
{
    switch (args[0])
    {
        case "server":
        {
            var server = new Server();
            server.Start().Wait();
            break;
        }
        case "client":
        {
            var client = new Client();
            client.Start().Wait();
            break;
        }
        default:
        {
            Console.WriteLine("Invalid input");
            break;
        }
    }
    
    
}