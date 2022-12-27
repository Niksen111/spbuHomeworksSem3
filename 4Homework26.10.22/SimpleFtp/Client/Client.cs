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
    /// <param name="path">Path to the directory.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<string?> ListAsync(NetworkStream stream, string path)
    {
        await using var writer = new StreamWriter(stream);

        await writer.WriteLineAsync($"1 {path}");
        await writer.FlushAsync();

        using var reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();
        if (result == "-1")
        {
            throw new DirectoryNotFoundException();
        }

        return result;
    }

    /// <summary>
    /// Sends a request for a file to the server.
    /// </summary>
    /// <param name="stream">The stream to read and write to.</param>
    /// <param name="path">Path to file.</param>
    /// <param name="newPath">Path at which file should be saved.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task GetAsync(NetworkStream stream, string path, string newPath)
    {
        await using var writer = new StreamWriter(stream);

        await writer.WriteLineAsync($"2 {path}");
        await writer.FlushAsync();

        var byteLength = new byte[8];
        await stream.ReadAsync(byteLength);
        var length = BitConverter.ToInt32(byteLength);
        if (length == -1)
        {
            throw new FileNotFoundException();
        }

        using var file = File.Open(newPath, FileMode.Open);
        await stream.CopyToAsync(file);
    }
}