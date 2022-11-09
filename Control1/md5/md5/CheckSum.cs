using System.ComponentModel.DataAnnotations;
using System.Text;
using static System.Security.Cryptography.MD5;

namespace md5;

/// <summary>
/// Calculates check sum
/// </summary>
public static class CheckSum
{
    /// <summary>
    /// Calculates check sum
    /// </summary>
    /// <param name="path">path to file</param>
    /// <returns></returns>
    public static byte[]? GetCheckSum(string path)
    {
        if (File.Exists(path))
        {
            var data = File.ReadAllBytes(path);
            var md5 = HashData(data);
            return md5;
        }

        if (Directory.Exists(path))
        {
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            var temporary = Path.GetDirectoryName(path);
            var result = Encoding.UTF8.GetBytes(temporary!);
            Array.Sort(files);
            Array.Sort(directories);

            for (int i = 0; i < files.Length; ++i)
            {
                var currentBytes = GetCheckSum(files[i]);
                result.Concat(currentBytes!).ToArray();
            }

            for (int i = 0; i < directories.Length; ++i)
            {
                var currentBytes = GetCheckSum(directories[i]);
                result.Concat(currentBytes!).ToArray();
            }

            return HashData(result);
        }
        
        return null;
    }

    /// <summary>
    /// Calculates check sum parallel
    /// </summary>
    /// <param name="path">path to file</param>
    /// <returns></returns>
    public static async Task<byte[]> GetCheckSumParallel(string path)
    {
        if (File.Exists(path))
        {
            var data = File.ReadAllBytesAsync(path);
            var md5 = HashData(await data);
            return md5;
        }

        if (Directory.Exists(path))
        {
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            var temporary = Path.GetDirectoryName(path);
            var result = Encoding.UTF8.GetBytes(temporary!);
            Array.Sort(files);
            Array.Sort(directories);
            var filesProcesses = new Task<byte[]>[files.Length];
            var directoryProcesses = new Task<byte[]>[directories.Length];

            for (int i = 0; i < files.Length; ++i)
            {
                filesProcesses[i] = GetCheckSumParallel(files[i]);
            }

            for (int i = 0; i < directories.Length; ++i)
            {
                directoryProcesses[i] = GetCheckSumParallel(directories[i]);
            }
            
            for (int i = 0; i < files.Length; ++i)
            {
                result.Concat(await filesProcesses[i]).ToArray();
            }

            for (int i = 0; i < directories.Length; ++i)
            {
                result.Concat(await directoryProcesses[i]).ToArray();
            }

            return HashData(result);
        }

        throw new FileNotFoundException();
    }
}