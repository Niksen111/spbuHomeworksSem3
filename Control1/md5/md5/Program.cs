using System.Diagnostics;
using static System.Diagnostics.Stopwatch;


namespace md5;

public class Program
{
    public static void Main(string[] args)
    {
        long time1;
        long time2;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var x1 = CheckSum.GetCheckSum("../../../../../../");
        stopwatch.Stop();
        time1 = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        stopwatch.Start();
        var x2 = CheckSum.GetCheckSumParallel("../../../../../../").Result;
        stopwatch.Stop();
        time2 = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"One-thread {time1}, parallel {time2}");
    }
}