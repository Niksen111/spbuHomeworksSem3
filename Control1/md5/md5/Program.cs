namespace md5;

public class Program
{
    public static void Main(string[] args)
    {
        var x1 = CheckSum.GetCheckSum("../../");
        var x2 = CheckSum.GetCheckSumParallel("../../").Result;

        Console.WriteLine();
    }
}