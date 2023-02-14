namespace SimpleFtp.Tests;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Client = Client.Client;
using Server = Server.Server;

public class ClientTests
{
    private Server? server;
    private CancellationTokenSource? source;
    private Client? client;
    private Task? serverState;
    private int port = 7777;
    private string simpleFtpTestsListing = "6 ../../../bin true ../../../obj true ../../../TestingFiles true ../../../ClientTests.cs false ../../../ServerTests.cs false ../../../SimpleFtpTests.csproj false ";

    [SetUp]
    public void SetUp()
    {
        this.client = new Client(this.port);
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        this.source = new CancellationTokenSource();
        this.server = new Server();
        this.serverState = Task.Run(() => this.server.Start(this.source.Token, this.port), this.source.Token);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        this.source!.Cancel();
        await this.serverState!;
    }

    [Test]
    public void GetsListing()
    {
        var contents1 = this.client!.ListAsync("../../").Result;
        var result1 = contents1.Count + " ";
        foreach (var content in contents1)
        {
            result1 += content.ToString();
        }

        var contents2 = this.client!.ListAsync("../../../").Result;
        var result2 = contents2.Count + " ";
        foreach (var content in contents2)
        {
            result2 += content.ToString();
        }

        var contents3 = this.client!.ListAsync("../../../TestingFiles").Result;
        var result3 = contents3.Count + " ";
        foreach (var content in contents3)
        {
            result3 += content.ToString();
        }

        Assert.AreEqual("1 ../../Debug true ", result1);
        Assert.AreEqual(this.simpleFtpTestsListing, result2);
        Assert.AreEqual(
            $"4 ../../../TestingFiles{Path.DirectorySeparatorChar}ABCD.axax false ../../../TestingFiles{Path.DirectorySeparatorChar}ABCD1.axax false ../../../TestingFiles{Path.DirectorySeparatorChar}kek.txt false ../../../TestingFiles{Path.DirectorySeparatorChar}kek1.txt false ",
            result3);
    }

    [Test]
    public async Task DownloadsFile()
    {
        await using var out1 = File.OpenWrite($"../../../TestingFiles{Path.DirectorySeparatorChar}kek1.txt");
        await using var out2 = File.OpenWrite($"../../../TestingFiles{Path.DirectorySeparatorChar}ABCD1.axax");

        await this.client!.GetAsync($"../../../TestingFiles{Path.DirectorySeparatorChar}kek.txt", out1);
        await this.client!.GetAsync($"../../../TestingFiles{Path.DirectorySeparatorChar}ABCD.axax", out2);

        out1.Close();
        out2.Close();

        Assert.IsTrue(this.FileCompare($"../../../TestingFiles{Path.DirectorySeparatorChar}kek.txt", $"../../../TestingFiles{Path.DirectorySeparatorChar}kek1.txt"));
        Assert.IsTrue(this.FileCompare($"../../../TestingFiles{Path.DirectorySeparatorChar}ABCD.axax", $"../../../TestingFiles{Path.DirectorySeparatorChar}ABCD1.axax"));
    }

    private bool FileCompare(string file1, string file2)
    {
        int file1Byte;
        int file2Byte;

        if (file1 == file2)
        {
            return true;
        }

        using var fs1 = new FileStream(file1, FileMode.Open);
        using var fs2 = new FileStream(file2, FileMode.Open);

        if (fs1.Length != fs2.Length)
        {
            fs1.Close();
            fs2.Close();

            return false;
        }

        do
        {
            file1Byte = fs1.ReadByte();
            file2Byte = fs2.ReadByte();
        }
        while (file1Byte == file2Byte && file1Byte != -1);

        fs1.Close();
        fs2.Close();

        return file1Byte - file2Byte == 0;
    }
}