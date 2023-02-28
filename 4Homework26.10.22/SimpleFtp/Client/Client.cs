namespace Client;

using System.Net.Sockets;

/// <summary>
/// Client allowing to execute requests:
/// List - listing files in the directory on the server
/// Get - downloading a file from the server.
/// </summary>
public class Client
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="port">The port number of the remote host to which you intend to connect.</param>
    public Client(int port = 11111)
        : this("localhost", port)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="host">The DNS name of the remote host to which you intend to connect.</param>
    /// <param name="port">The port number of the remote host to which you intend to connect.</param>
    public Client(string host, int port = 11111)
    {
        this.Host = host;
        this.Port = port;
    }

    /// <summary>
    /// Gets the DNS name of the remote host to which you intend to connect.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the port number of the remote host to which you intend to connect.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Sends a request for a listing of the directory to the server.
    /// </summary>
    /// <param name="path">Path to the target directory.</param>
    /// <exception cref="InvalidOperationException">The Client is not connected to a remote host.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory with the specified path was not found on the server.</exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<List<Record>> ListAsync(string path)
    {
        var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(this.Host, this.Port);
        await using var stream = tcpClient.GetStream();
        await using var writer = new StreamWriter(stream);
        using var reader = new StreamReader(stream);
        await writer.WriteLineAsync($"1 {path}");
        await writer.FlushAsync();

        var line = await reader.ReadLineAsync();
        if (line == null)
        {
            throw new IOException();
        }

        if (line == "-1")
        {
            throw new DirectoryNotFoundException();
        }

        var line1 = line.Split();
        var contents = new List<Record>();
        for (int i = 1; i < line1.Length - 1; i += 2)
        {
            contents.Add(new Record(line1[i], line1[i + 1] == "true"));
        }

        return contents;
    }

    /// <summary>
    /// Sends a request for a file to the server and returns it.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <param name="outStream">The stream into which the received file will be written.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task GetAsync(string path, Stream outStream)
    {
        var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(this.Host, this.Port);
        await using var stream = tcpClient.GetStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync($"2 {path}");
        await writer.FlushAsync();

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
        while (leftToRead > bufferSize)
        {
            leftToRead -= bufferSize;
            wasRead = await stream.ReadAsync(buffer, 0, bufferSize);
            if (wasRead != bufferSize)
            {
                throw new IOException();
            }

            await outStream.WriteAsync(buffer);
        }

        buffer = new byte[(int)leftToRead];
        wasRead = await stream.ReadAsync(buffer, 0, (int)leftToRead);
        if (wasRead != (int)leftToRead)
        {
            throw new IOException();
        }

        await outStream.WriteAsync(buffer, 0, (int)leftToRead);
        await outStream.FlushAsync();
    }

    /// <summary>
    /// Abstraction of directory contents (files or directories).
    /// </summary>
    public record struct Record
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Record"/> struct.
        /// </summary>
        /// <param name="name">Record name.</param>
        /// <param name="isDirectory">A value indicating whether the record is a directory.</param>
        public Record(string name, bool isDirectory)
        {
            this.Name = name;
            this.IsDirectory = isDirectory;
        }

        /// <summary>
        /// Gets record name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether gets true if the record is directory.
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// Converts DirectoryContent to string.
        /// </summary>
        /// <returns>String version of the DirectoryContent.</returns>
        public override string ToString()
        {
            return this.Name + (this.IsDirectory ? " true " : " false ");
        }
    }
}