﻿namespace Server;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// A server handling two requests:
/// List - listing files in the directory on the server
/// Get - downloading a file from the server.
/// </summary>
public class Server
{
    private CancellationToken token;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="token">Token to stop listening.</param>
    public Server(CancellationToken token)
    {
        this.token = token;
    }

    /// <summary>
    /// Start server.
    /// </summary>
    /// <param name="port">The port on which to listen for incoming connection attempts.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task Start(int port = 11111)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        while (!this.token.IsCancellationRequested)
        {
            var socket = await listener.AcceptSocketAsync();
            await Task.Run(() => this.Session(socket));
        }
    }

    private async Task Session(Socket socket)
    {
        try
        {
            var stream = new NetworkStream(socket);
            var listener = new StreamReader(stream);
            var writer = new StreamWriter(stream);
            while (true)
            {
                var line = await listener.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                var request = line.Split();

                if (request.Length != 2)
                {
                    await writer.WriteLineAsync("Incorrect request");
                    await writer.FlushAsync();
                    continue;
                }

                switch (request[0])
                {
                    case "1":
                    {
                        await this.ListAsync(request[1], writer);
                        break;
                    }

                    case "2":
                    {
                        await this.GetAsync(request[1], writer, stream);
                        break;
                    }

                    default:
                    {
                        await writer.WriteLineAsync("Incorrect request");
                        await writer.FlushAsync();
                        continue;
                    }
                }
            }
        }
        finally
        {
            socket.Close();
        }
    }

    private async Task ListAsync(string path, StreamWriter writer)
    {
        if (!Directory.Exists(path))
        {
            await writer.WriteLineAsync("-1");
            await writer.FlushAsync();
            return;
        }

        List<string> directories = new List<string>(Directory.GetDirectories(path));
        List<string> files = new List<string>(Directory.GetFiles(path));
        directories.Sort();
        files.Sort();
        await writer.WriteAsync($"{directories.Count + files.Count} ");
        foreach (var directory in directories)
        {
            await writer.WriteAsync($"{path}{directory} true ");
        }

        foreach (var file in files)
        {
            await writer.WriteAsync($"{path}{file} false ");
        }

        await writer.WriteLineAsync();

        await writer.FlushAsync();
    }

    private async Task GetAsync(string path, StreamWriter writer, NetworkStream stream)
    {
        if (!File.Exists(path))
        {
            await writer.WriteLineAsync("-1");
            await writer.FlushAsync();
            return;
        }

        var info = new FileInfo(path);
        var file = info.Open(FileMode.Open);
        await file.CopyToAsync(stream, this.token);
        file.Close();
    }
}