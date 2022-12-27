using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Server = Server.Server;

namespace SimpleFtp.Tests;

public class Tests
{
    private CancellationToken token;
    private global::Server.Server server;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        token = new();
        server = new global::Server.Server(token);
    }
    
    [Test]
    public void Test1()
    {

    }
}