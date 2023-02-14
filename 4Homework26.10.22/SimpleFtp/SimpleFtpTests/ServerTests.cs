namespace SimpleFtp.Tests;

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

public class ServerTests
{
    private Server.Server? server;
    private CancellationTokenSource? source;
    private int port = 6666;
    private Task? serverState;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        this.source = new CancellationTokenSource();
        this.server = new Server.Server();
        this.serverState = Task.Run(() => this.server.Start(this.source.Token, this.port), this.source.Token);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        this.source!.Cancel();
        await this.serverState!;
    }

    [Test]
    public void ListingWorks()
    {
        using var client = new TcpClient();
        client.Connect("127.0.0.1", this.port);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream);
        using var reader = new StreamReader(stream);
        writer.WriteLine("1 ../../");
        writer.Flush();
        var response = reader.ReadLine();
        Assert.AreEqual("1 ../../Debug true ", response);

        writer.WriteLine("1 ../");
        writer.Flush();
        response = reader.ReadLine();
        Assert.AreEqual("1 ../net6.0 true ", response);
    }

    [Test]
    public void GetWorks()
    {
        using var client = new TcpClient();
        client.Connect("127.0.0.1", this.port);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream);
        writer.WriteLine("2 ../../../TestingFiles/kek.txt");
        writer.Flush();
        var response = this.ReadFileFromStream(stream);
        Assert.AreEqual("MathMech isn't for everyone", response.Result);
    }

    [Test]
    public void ServerWorksWithSeveralClients()
    {
        Assert.IsTrue(this.server!.IsWorking);
        using var client1 = new TcpClient();
        client1.Connect("127.0.0.1", this.port);
        using var stream1 = client1.GetStream();
        using var writer1 = new StreamWriter(stream1);
        using var reader1 = new StreamReader(stream1);

        using var client2 = new TcpClient();
        client2.Connect("127.0.0.1", this.port);
        using var stream2 = client2.GetStream();
        var writer2 = new StreamWriter(stream2);
        var reader2 = new StreamReader(stream2);

        using var client3 = new TcpClient();
        client3.Connect("127.0.0.1", this.port);
        using var stream3 = client3.GetStream();
        using var writer3 = new StreamWriter(stream3);
        using var reader3 = new StreamReader(stream3);

        writer1.WriteLine("1 ../../");
        writer1.Flush();
        var response = reader1.ReadLine();
        Assert.AreEqual("1 ../../Debug true ", response);

        writer2.WriteLine("1 ../");
        writer2.Flush();
        response = reader2.ReadLine();
        Assert.AreEqual("1 ../net6.0 true ", response);

        writer3.WriteLine("1 ../../");
        writer3.Flush();
        response = reader3.ReadLine();
        Assert.AreEqual("1 ../../Debug true ", response);

        writer2.WriteLine("1 ../");
        writer2.Flush();
        response = reader2.ReadLine();
        Assert.AreEqual("1 ../net6.0 true ", response);
        Assert.IsTrue(this.server!.IsWorking);
    }

    [Test]
    public void NonexistentFileRequestWorks()
    {
        using var client = new TcpClient();
        client.Connect("127.0.0.1", this.port);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream);
        using var reader = new StreamReader(stream);
        writer.WriteLine("2 ./abdfdf.sus");
        writer.Flush();
        var response = reader.ReadLine();
        Assert.AreEqual("-1", response);
    }

    private async Task<string?> ReadFileFromStream(NetworkStream stream)
    {
        var byteLength = new byte[8];
        var wasRead = await stream.ReadAsync(byteLength, 0, 8);
        if (wasRead != 8)
        {
            throw new IOException();
        }

        var length = BitConverter.ToInt64(byteLength);
        if (length == -1)
        {
            throw new FileNotFoundException();
        }

        wasRead = await stream.ReadAsync(byteLength, 0, 1);
        if (wasRead != 1)
        {
            throw new IOException();
        }

        var leftToRead = length;
        var bufferSize = 1000000;
        var buffer = new byte[bufferSize];
        var response = string.Empty;
        while (leftToRead > bufferSize)
        {
            leftToRead -= bufferSize;
            wasRead = await stream.ReadAsync(buffer, 0, bufferSize);
            if (wasRead != bufferSize)
            {
                throw new IOException();
            }

            response += System.Text.Encoding.Default.GetString(buffer);
        }

        buffer = new byte[(int)leftToRead];
        wasRead = await stream.ReadAsync(buffer, 0, (int)leftToRead);
        if (wasRead != (int)leftToRead)
        {
            throw new IOException();
        }

        response += System.Text.Encoding.UTF8.GetString(buffer);
        return response;
    }
}