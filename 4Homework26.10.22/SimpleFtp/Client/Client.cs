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
    /// Sends a request for a listing of the directory to the server.
    /// </summary>
    /// <param name="stream">The stream to read and write to.</param>
    /// <param name="path">Path to the target directory.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<string?> ListAsync(NetworkStream stream, string path)
    {
        var writer = new StreamWriter(stream);
        var reader = new StreamReader(stream);
        await writer.WriteLineAsync($"1 {path}");
        await writer.FlushAsync();

        var result = await reader.ReadLineAsync();
        if (result == "-1")
        {
            throw new DirectoryNotFoundException();
        }

        return result;
    }

    /// <summary>
    /// Sends a request for a file to the server and returns it.
    /// </summary>
    /// <param name="stream">The stream to read and write to.</param>
    /// <param name="path">Path to file.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<string?> GetAsync(NetworkStream stream, string path)
    {
        var writer = new StreamWriter(stream);
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

    /// <summary>
    /// Sends a request for a file to the server and downloads it.
    /// </summary>
    /// <param name="stream">The stream to read and write to.</param>
    /// <param name="path">Path to file.</param>
    /// <param name="newPath">Path at which file should be saved.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DownloadAsync(NetworkStream stream, string path, string newPath)
    {
        var writer = new StreamWriter(stream);
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

        await using var file = new FileStream(newPath, FileMode.Create);

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

            await file.WriteAsync(buffer);
        }

        buffer = new byte[(int)leftToRead];
        wasRead = await stream.ReadAsync(buffer, 0, (int)leftToRead);
        if (wasRead != (int)leftToRead)
        {
            throw new IOException();
        }

        await file.WriteAsync(buffer, 0, (int)leftToRead);
        file.Close();
    }
}