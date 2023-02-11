using System;
using System.IO;

namespace SimpleFtp.Tests;

using System.Net.Sockets;
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
    private int port = 7777;
    private string simpleFtpTestsListing = "6 ../../../bin true ../../../obj true ../../../TestingFiles true ../../../ClientTests.cs false ../../../ServerTests.cs false ../../../SimpleFtpTests.csproj false ";

    [SetUp]
    public void SetUp()
    {
        this.client = new Client();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        this.source = new CancellationTokenSource();
        this.server = new Server(this.source.Token);
        Task.Run(() => this.server.Start(this.port));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        this.source!.Cancel();
    }

    [Test]
    public void GetsListing()
    {
        using var tcpClient = new TcpClient();
        tcpClient.Connect("127.0.0.1", this.port);
        using var stream = tcpClient.GetStream();

        Assert.AreEqual("1 ../../Debug true ", this.client!.ListAsync(stream, "../../").Result);
        Assert.AreEqual(this.simpleFtpTestsListing, this.client!.ListAsync(stream, "../../../").Result);
        Assert.AreEqual(
            "4 ../../../TestingFiles/ABCD.cmf false ../../../TestingFiles/ABCD1.cmf false ../../../TestingFiles/kek.txt false ../../../TestingFiles/kek1.txt false ", 
            this.client!.ListAsync(stream, "../../../TestingFiles").Result);
    }

    [Test]
    public void GetsFile()
    {
        using var tcpClient = new TcpClient();
        tcpClient.Connect("127.0.0.1", this.port);
        using var stream = tcpClient.GetStream();

        Assert.AreEqual("MathMech isn't for everyone", this.client!.GetAsync(stream, "../../../TestingFiles/kek.txt").Result);
        Assert.AreEqual("MathMech is the best", this.client!.GetAsync(stream, "../../../TestingFiles/ABCD.cmf").Result);
    }

    [Test]
    public async Task DownloadsFile()
    {
        using var tcpClient = new TcpClient();
        tcpClient.Connect("127.0.0.1", this.port);
        using var stream = tcpClient.GetStream();

        await this.client!.DownloadAsync(stream, "../../../TestingFiles/kek.txt", "../../../TestingFiles/kek1.txt");
        await this.client!.DownloadAsync(stream, "../../../TestingFiles/ABCD.cmf", "../../../TestingFiles/ABCD1.cmf");

        Assert.IsTrue(this.FileCompare("../../../TestingFiles/kek.txt", "../../../TestingFiles/kek1.txt"));
        Assert.IsTrue(this.FileCompare("../../../TestingFiles/ABCD.cmf", "../../../TestingFiles/ABCD1.cmf"));
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